using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            // Add mappings here as needed for lookup entities
        }
    }
}
