using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(30, ErrorMessage ="Category can't be greater than 30 characters")]
        public string Name { get; set; }

        // Navigation: One User can have many Todos
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();

    }
}
