namespace PassManNew.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }  
        public int RefNo { get; set; }  
        public string Address { get; set; }  
        public DateTime StartDate { get; set; }  
        public DateTime EndDate { get; set; }  
        public int ClientProjectManagerId { get; set; }  
        public string ClientManagerName { get; set; }  
        public int AzardProjectManagerId { get; set; }  
        public string AzardManagerName { get; set; }  
        public string State { get; set; }
    }
}
