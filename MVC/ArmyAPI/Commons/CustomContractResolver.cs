using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace ArmyAPI.Commons
{
	public class CustomContractResolver : DefaultContractResolver
	{
		private readonly HashSet<string> _propertiesToIgnore;

		public CustomContractResolver(params string[] propertiesToIgnore)
		{
			_propertiesToIgnore = new HashSet<string>(propertiesToIgnore, StringComparer.OrdinalIgnoreCase);
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

			// 只保留不在忽略清單中的屬性
			return properties.Where(p => !_propertiesToIgnore.Contains(p.PropertyName)).ToList();
		}
	}
}