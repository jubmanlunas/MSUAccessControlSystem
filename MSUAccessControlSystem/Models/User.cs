public class User
{
    public string RFIDUID { get; set; }
    public string IDNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Name => $"{FirstName} {LastName}";
    public string Department { get; set; }
    public string CourseYrPosition { get; set; }
    public string UserType { get; set; }
    public byte[] Photo { get; set; }
}