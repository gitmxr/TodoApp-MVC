using Microsoft.AspNetCore.Identity;

namespace TodoApp.Models
{
    public class User : IdentityUser
    {
        // Custom fields you want to add
        public string FullName { get; set; } = string.Empty;

        // Relation: one user → many todos
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}
