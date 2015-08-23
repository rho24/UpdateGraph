using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace UpdateGraph.Specs
{
    public class EfMemorySpecs
    {
        #region Nested type: Db

        public class Db : DbContext
        {
            public DbSet<Post> Posts { get; set; }

            public Db(DbContextOptions<Db> options)
                : base(options) {}

            public static Db CreateInMemory() {
                var builder = new DbContextOptionsBuilder<Db>();
                builder.UseInMemoryDatabase(persist: true);
                var options = builder.Options;

                var db = new Db(options);

                return db;
            }

            #region Nested type: Post

            public class Post
            {
                public int Id { get; set; }

                public string Name { get; set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: When_Using_Seperate_Contexts

        public class When_using_seperate_contexts
        {
            static Db.Post _post;

            Because of = () => {
                using(var db = Db.CreateInMemory()) {
                    db.Add(new Db.Post() { Name = "Bob" });

                    db.SaveChanges();
                }
                using (var db2 = Db.CreateInMemory()) {
                    _post = db2.Posts.FirstOrDefault();
                }
            };

            It Should_Keep_History = () => _post.ShouldNotBeNull();
        }

        public class When_starting_a_new_spec
        {
            static Db.Post _post;

            Because of = () => {
                using (var db2 = Db.CreateInMemory())
                {
                    _post = db2.Posts.FirstOrDefault();
                }
            };

            It Should_Not_Keep_History = () => _post.ShouldBeNull();
        }

        #endregion
    }
}