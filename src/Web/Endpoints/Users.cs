#if (UseApiOnly)
using IgnaCheck.Infrastructure.Identity;

namespace IgnaCheck.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
    }
}
#endif
