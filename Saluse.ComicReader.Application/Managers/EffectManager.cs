using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Application.Managers
{
	class EffectManager
	{
		private IList<Type> _shaderEffectTypes;
		private int _currentIndex;

		public EffectManager()
		{
			_currentIndex = 0;

			BuildEffectsList();
		}

		private void BuildEffectsList()
		{
			var shaderEffectType = typeof(ShaderEffect);
			var assembly = AppDomain.CurrentDomain.Load("Saluse.ComicReader.Effects");

			// Get all types for the Effects assembly. Only return types that inherit from ShaderEffect type
			// and also does not have an ExcludeEffect attribute
			_shaderEffectTypes = assembly
														.GetTypes()
														.Where(x => x.IsSubclassOf(shaderEffectType) && !x.CustomAttributes.Any(attribute => attribute.AttributeType.Name == "ExcludeEffectAttribute"))
														.OrderBy(x => x.Name)
														.ToArray();
		}

		public ShaderEffect GetShaderEffect(int index)
		{
			ShaderEffect shaderEffect = null;

			if (index < this.Count)
			{
				shaderEffect = (ShaderEffect)Activator.CreateInstance(_shaderEffectTypes[index]);
			}

			return shaderEffect;
		}

		public string GetEffectName(ShaderEffect shaderEffect)
		{
			string effectName = "no effect";
			if (shaderEffect != null)
			{
				var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(shaderEffect.GetType(), typeof(DescriptionAttribute));
				if (descriptionAttribute == null)
				{
					effectName = shaderEffect.GetType().Name;
				}
				else
				{
					effectName = descriptionAttribute.Description;
				}
			}

			return effectName;
		}

		public ShaderEffect GetNextEffect()
		{
			_currentIndex++;
			if (_currentIndex > this.Count)
			{
				_currentIndex = 0;
				return null;
			}
			else
			{
				return this.GetShaderEffect(_currentIndex - 1);
			}
		}

		public ShaderEffect GetPreviousEffect()
		{
			_currentIndex--;
			if (_currentIndex == 0)
			{
				return null;
			}
			else if (_currentIndex < 0)
			{
				_currentIndex = this.Count;
			}

			return this.GetShaderEffect(_currentIndex - 1);
		}

		public int Count
		{
			get
			{
				return _shaderEffectTypes.Count;
			}
		}
	}
}
