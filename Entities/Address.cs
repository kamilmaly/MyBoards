using Microsoft.EntityFrameworkCore;

namespace MyBoards.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Country { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string PostalCode { get; set; }

        public User User { get; set; }

        public Guid UserId { get; set; }

        public Coordinate Coordinate { get; set; }
    }

    public class Coordinate
    {
        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set;}
    }
}
