using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader.Application.Models
{
	public class InformationModel : ViewModel
	{
		private string _informationText;
		private int _currentPageNumber;
		private int _pageTotal;

		public int CurrentPageNumber
		{
			get
			{
				return _currentPageNumber;
			}

			set
			{
				_currentPageNumber = value;
				this.Notify("CurrentPageNumber");
				this.Notify("PageInformation");
			}
		}

		public int PageTotal
		{
			get
			{
				return _pageTotal;
			}
			set
			{
				_pageTotal = value;
				this.Notify("PageTotal");
				this.Notify("PageInformation");
			}
		}

		public string PageInformation
		{
			get
			{
				return string.Format("{0} / {1}", this.CurrentPageNumber, this.PageTotal);
			}
		}
		public string InformationText
		{
			get
			{
				return _informationText;
			}
			set
			{
				_informationText = value;
				this.Notify("InformationText");
			}
		}
	}
}
