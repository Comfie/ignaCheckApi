using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using IgnaCheck.Application.Common.Interfaces;
using NUnit.Framework;

namespace IgnaCheck.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config => 
            config.AddMaps(Assembly.GetAssembly(typeof(IApplicationDbContext))));

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    // Add mapping test cases here as needed
    // [Test]
    // [TestCase(typeof(SourceEntity), typeof(DestinationDto))]
    // public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    // {
    //     var instance = GetInstanceOf(source);
    //     _mapper.Map(instance, source, destination);
    // }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }
}
