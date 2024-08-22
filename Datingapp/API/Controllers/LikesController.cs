using Dapper;
using Datingapp.API.Data;
using Datingapp.API.DTO;
using Datingapp.API.Extensions;
using Datingapp.API.Helpers;
using Datingapp.API.Interface;
using Datingapp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Datingapp.API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly DapperDbContext dapperDbContext;
        private readonly DataContext context;

        //private readonly UserRepository userRepository;
        //private readonly ILikesRepository likesRepository;

        public LikesController(IUnitOfWork unitOfWork, DapperDbContext dapperDbContext, DataContext context
            //UserRepository userRepository, ILikesRepository likesRepository
            )
        {
            //this.userRepository = userRepository;
            //this.likesRepository = likesRepository;
            this.unitOfWork = unitOfWork;
            this.dapperDbContext = dapperDbContext;
            this.context = context;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await unitOfWork.UserRepository.GetByUsername(username);
            var sourceUser = await unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id,
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("match")]
        public async Task<IEnumerable<LikeDto>> GetMatch(int userId)
        {
            userId = User.GetUserId();

            string sql = @"SELECT l1.SourceUserId AS UserA,
                l1.LikedUserId AS UserB,
                u2.*,
                    DATEDIFF(YEAR, u2.DateOfBirth, GETDATE()) - 
                    CASE 
                    WHEN MONTH(u2.DateOfBirth) > MONTH(GETDATE()) 
                    OR (MONTH(u2.DateOfBirth) = MONTH(GETDATE()) AND DAY(u2.DateOfBirth) > DAY(GETDATE()))
                    THEN 1 
                    ELSE 0 
                    END AS Age,
                p.Url,p.AppUserId
                FROM Likes l1
                INNER JOIN Likes l2
                ON l1.SourceUserId = l2.LikedUserId
                    AND l1.LikedUserId = l2.SourceUserId
                INNER JOIN AspNetUsers u2
                ON l1.LikedUserId = u2.Id
                LEFT JOIN Photos p
                ON u2.Id = p.AppUserId
                WHERE l1.SourceUserId = @SourceUserId;";

            using (var connection = dapperDbContext.CreateConnection())
            {
                var parameter = new { SourceUserId = userId };
                //var parameter = new SqlParameter("@SourceUserId", userId);
                //var result = await connection.QueryAsync<MutualLike>(sql, parameter);
                var result = await connection.QueryAsync<LikeDto>(sql, parameter);

                return result;
            }
        }
    }


}

