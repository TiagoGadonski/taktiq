using System.Security.Claims;
using GymHero.Application.Common.Exceptions;
using GymHero.Application.Features.Posts.Commands;
using GymHero.Application.Features.Posts.Queries;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        // Personal Trainer endpoints - require PT authorization
        var personalGroup = app.MapGroup("/api/personal/posts")
            .WithTags("Personal Trainer - Posts")
            .RequireAuthorization("RequirePersonalRole");

        // Create a new post
        personalGroup.MapPost("", async (
            [FromBody] CreatePostRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreatePostCommand(
                authorId,
                request.Title,
                request.Content,
                request.ImageUrl,
                request.IsPublished
            );

            var postId = await sender.Send(command);
            return Results.Created($"/api/posts/{postId}", new { id = postId });
        })
        .WithName("CreatePost")
        .WithSummary("Creates a new blog post");

        // Get all posts by the authenticated trainer
        personalGroup.MapGet("", async (
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetMyPostsQuery(authorId);
            var posts = await sender.Send(query);
            return Results.Ok(posts);
        })
        .WithName("GetMyPosts")
        .WithSummary("Gets all posts by the authenticated personal trainer");

        // Update an existing post
        personalGroup.MapPut("/{postId:guid}", async (
            Guid postId,
            [FromBody] UpdatePostRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new UpdatePostCommand(
                postId,
                authorId,
                request.Title,
                request.Content,
                request.ImageUrl,
                request.IsPublished
            );

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UpdatePost")
        .WithSummary("Updates an existing post");

        // Delete a post
        personalGroup.MapDelete("/{postId:guid}", async (
            Guid postId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new DeletePostCommand(postId, authorId);

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("DeletePost")
        .WithSummary("Deletes a post");

        // Public endpoints - anyone can view published posts
        var publicGroup = app.MapGroup("/api/posts")
            .WithTags("Posts - Public")
            .AllowAnonymous();

        // Get a specific post by ID
        publicGroup.MapGet("/{postId:guid}", async (
            Guid postId,
            ISender sender) =>
        {
            try
            {
                var query = new GetPostByIdQuery(postId);
                var post = await sender.Send(query);
                return Results.Ok(post);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetPostById")
        .WithSummary("Gets a specific post by ID");

        // Get all published posts from a specific trainer
        publicGroup.MapGet("/trainer/{trainerId:guid}", async (
            Guid trainerId,
            ISender sender) =>
        {
            var query = new GetTrainerPostsQuery(trainerId);
            var posts = await sender.Send(query);
            return Results.Ok(posts);
        })
        .WithName("GetTrainerPosts")
        .WithSummary("Gets all published posts from a specific trainer");
    }
}
