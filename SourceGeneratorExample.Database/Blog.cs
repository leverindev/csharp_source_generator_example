using SourceGeneratorExample.DatabaseAttributes;

namespace SourceGeneratorExample.Database
{
    [Entity]
    public class Blog
    {
        [Field]
        [Key]
        public int Id { get; set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public int OwnerId { get; set; }
    }
}