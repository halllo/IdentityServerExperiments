using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Validation;

namespace IdentityServer.Extended
{
    public class ExtendedAuthorizeRequestValidator : IAuthorizeRequestValidator
    {
        private readonly Decorator<IAuthorizeRequestValidator> inner;

        public ExtendedAuthorizeRequestValidator(Decorator<IAuthorizeRequestValidator> inner)
        {
            this.inner = inner;
        }

        public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
        {
            var result = await inner.Instance.ValidateAsync(parameters, subject);

            return result;
        }
    }
}
