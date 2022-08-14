﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using Serilog;

namespace Plogon;

/// <summary>
/// Things that talk to the GitHub api
/// </summary>
public class GitHubApi
{
    private readonly HttpClient client;

    /// <summary>
    /// Make new GitHub API
    /// </summary>
    /// <param name="token">Github token</param>
    public GitHubApi(string token)
    {
        this.client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/vnd.github+json") },
                UserAgent = { new ProductInfoHeaderValue("PlogonBuild", "1.0.0") },
                Authorization = new AuthenticationHeaderValue("token", token),
            }
        };
    }

    private class CommentCreateBody
    {
        public string? Body { get; set; }
    }

    /// <summary>
    /// Add comment to issue
    /// </summary>
    /// <param name="repo">The repo to use</param>
    /// <param name="issueNumber">The issue number</param>
    /// <param name="body">The body</param>
    public async Task AddComment(string repo, int issueNumber, string body)
    {
        var jsonBody = new CommentCreateBody()
        {
            Body = body,
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.github.com/repos/{repo}/issues/{issueNumber}/comments");
        request.Content = JsonContent.Create(jsonBody);

        var response = await this.client.SendAsync(request);
        //Log.Verbose("{Repo}, {PrNum}: {Resp}", repo, issueNumber, await response.Content.ReadAsStringAsync());
        
        response.EnsureSuccessStatusCode();
    }

    private class IssueResponse
    {
        [JsonPropertyName("body")] public string? Body { get; } = null!;
    }

    /// <summary>
    /// Get the body of an issue.
    /// </summary>
    /// <param name="repo">Repo name</param>
    /// <param name="issueNumber">Issue number</param>
    /// <returns>PR body</returns>
    /// <exception cref="Exception">Thrown when the body couldn't be read</exception>
    public async Task<string> GetIssueBody(string repo, int issueNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.github.com/repos/{repo}/issues/{issueNumber}");
        var response = await this.client.SendAsync(request);
        //Log.Verbose("{Repo}, {PrNum}: {Resp}", repo, issueNumber, await response.Content.ReadAsStringAsync());
        
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<IssueResponse>();
        if (body?.Body == null)
            throw new Exception("Couldn't read issue body.");

        return body.Body;
    }
}