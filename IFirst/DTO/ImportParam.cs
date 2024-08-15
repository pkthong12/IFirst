namespace IFirst.DTO
{
    public class ImportParam
    {
        public required string Base64String { get; set; }
    }
    public class EmployeeDTO
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public DateTime BirthDay { get; set; }
        public string Mail { get; set; }
    }
}
