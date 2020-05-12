﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhotosApp.Data;

namespace PhotosApp.Services.Authorization
{
    public class MustOwnPhotoHandler : AuthorizationHandler<MustOwnPhotoRequirement>
    {
        private readonly IPhotoRepository photoRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MustOwnPhotoHandler(IPhotoRepository photoRepository, IHttpContextAccessor httpContextAccessor)
        {
            this.photoRepository = photoRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MustOwnPhotoRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // NOTE: IHttpContextAccessor позволяет получать HttpContext там, где это не получается сделать более явно.
            var httpContext = httpContextAccessor.HttpContext;
            // NOTE: RouteData содержит информацию о пути и параметрах запроса.
            // Ее сформировал UseRouting и к моменту авторизации уже отработал.
            var routeData = httpContext?.GetRouteData();

            var photoIdString = routeData?.Values["id"].ToString();
            if (!Guid.TryParse(photoIdString, out var photoId))
            {
                context.Fail();
                return;
            }

            if (await photoRepository.IsPhotoOwnerAsync(photoId, userId))
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }
}