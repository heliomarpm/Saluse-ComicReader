using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saluse.ComicReader.Effects
{
	/// <summary>
	///		Use this attribute if you don't want a Shader Effect to be dynamically loaded
	///		<remarks>
	///			Does not affect Visual Studio, Blend or any other general purpose WPF tool that work with Effects
	///		</remarks>
	/// </summary>
	class ExcludeEffectAttribute : Attribute
	{
	}
}
