using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Tourism.Models.Relations;


namespace Tourism.Models
{
    public class Room
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string number { get; set; }

        [Required]

        public int numberOfMembers { get; set; }


        [Required]

        public double cost { get; set; }

        [Required]

        public bool status { get; set; }

        

        [ForeignKey("Hotel")]
        public int hotelId { get; set; }


        public Hotel hotel { get; set; }
        public bool accepted { get; set; } = false;

        [Required]
        public DateTime dateAdded { get; set; }

    }
}
