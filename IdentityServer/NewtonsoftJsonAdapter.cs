using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace IdentityServer
{
	public static class NewtonsoftJsonAdapter
	{
		private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
		};

		public static ContentResult NewtonsoftJsonResult(this ControllerBase controller, object value)
		{
			return controller.Content(JsonConvert.SerializeObject(value, SerializerSettings), "application/json; charset=utf-8", Encoding.UTF8);
		}

		public class Binder : IModelBinder
		{
			public async Task BindModelAsync(ModelBindingContext bindingContext)
			{
				if (bindingContext == null)
				{
					throw new ArgumentNullException(nameof(bindingContext));
				}

				using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
				{
					var json = await reader.ReadToEndAsync();
					try
					{
						var inputObject = JsonConvert.DeserializeObject(json, bindingContext.ModelType, SerializerSettings);

						bindingContext.Result = ModelBindingResult.Success(inputObject);
					}
					catch (Exception)
					{
						bindingContext.ModelState.TryAddModelError(bindingContext.ModelType.Name, "cannot be deserialized");
					}
				}
			}
		}
	}
}
