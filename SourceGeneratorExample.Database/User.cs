using SourceGeneratorExample.DatabaseAttributes;

namespace SourceGeneratorExample.Database
{
    [Entity]
    public class User
    {
        [Field]
        [Key]
        public int Id { get; set; }

        [Field]
        public string Name { get; set; }
    }
}
