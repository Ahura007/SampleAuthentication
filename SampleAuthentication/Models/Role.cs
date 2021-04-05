using System;
using Microsoft.AspNetCore.Identity;

namespace SampleAuthentication.Models
{
	public class Role : IdentityRole
	{
		
		public Role(string roleName) : base(roleName)
		{
			this.CreationDate = DateTime.Now;
		}

		public DateTime CreationDate { get; private set; }

		// FOR ORM !
		private Role() { }
	}
}