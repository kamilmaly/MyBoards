namespace MyBoards.Entities
{
    public class WorkItemState
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public List<WorkItem> WorkItems { get; set; }
    }
}
