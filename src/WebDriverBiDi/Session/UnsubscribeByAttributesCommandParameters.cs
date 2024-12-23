// <copyright file="UnsubscribeByAttributesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
public class UnsubscribeByAttributesCommandParameters : UnsubscribeCommandParameters
{
    private readonly List<string> eventList = [];

    private readonly List<string> contextList = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByAttributesCommandParameters"/> class.
    /// </summary>
    public UnsubscribeByAttributesCommandParameters()
        : base()
    {
    }

    /// <summary>
    /// Gets the list of events to which to subscribe or unsubscribe.
    /// </summary>
    [JsonPropertyName("events")]
    public List<string> Events => this.eventList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events.
    /// </summary>
    // TODO (issue #36): Remove obsolete property when removed from specification.
    [Obsolete("This property will be removed when it is removed from the W3C WebDriver BiDi Specification (see https://w3c.github.io/webdriver-bidi/#type-session-UnsubscribeByAttributesRequest)")]
    [JsonIgnore]
    public List<string> Contexts => this.contextList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to subscribe to or unsubscribe from the specified events for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.contextList.Count == 0)
            {
                return null;
            }

            return this.contextList;
        }
    }
}
