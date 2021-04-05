namespace SampleAuthentication.Dtos
{
	public class RoleDto
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string CreationDate { get; set; }

		public int TotalUsersInRoleCount { get; set; }
	}
}