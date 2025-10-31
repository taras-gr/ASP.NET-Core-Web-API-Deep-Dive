using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly string _requesHeaderToMatch;
    private readonly MediaTypeCollection _mediaTypes = new();

    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
        string mediaType, params string[] otherMediaTypes)
    {
        _requesHeaderToMatch = requestHeaderToMatch
            ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

        if (MediaTypeHeaderValue.TryParse(mediaType, 
            out var mediaTypeHeaderValue))
        {
            _mediaTypes.Add(mediaTypeHeaderValue);
        }
        else
        {
            throw new ArgumentException(nameof(mediaType));
        }

        foreach (var otherMediaType in otherMediaTypes)
        {
            if (MediaTypeHeaderValue.TryParse(otherMediaType,
                out var parsedOtherMediaType))
            {
                _mediaTypes.Add(parsedOtherMediaType);
            }
            else
            {
                throw new ArgumentException(nameof(otherMediaType));
            }
        }
    }

    public int Order { get; }

    public bool Accept(ActionConstraintContext context)
    {
        var requestHeader = context.RouteContext.HttpContext.Request.Headers;
        if (!requestHeader.ContainsKey(_requesHeaderToMatch))
        {
            return false;
        }

        var parsedRequestMediaType = new MediaType(requestHeader[_requesHeaderToMatch]);

        foreach (var mediaType in _mediaTypes)
        {
            var parsedMediaType = new MediaType(mediaType);
            if (parsedMediaType.Equals(parsedRequestMediaType))
            {
                return true;
            }
        }

        return false;
    }
}
