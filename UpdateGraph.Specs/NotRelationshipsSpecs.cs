using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace UpdateGraph.Specs
{
    public class NotRelationshipsSpecs
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

        #region Nested type: When_Updating_Exisitng_Entity

        public class When_updating_an_existing_entity
        {
            static int postId;
            static Db.Post _post;

            Establish context = () => {
                using(var db = Db.CreateInMemory()) {
                    var post = new Db.Post() { Name = "Bob" };
                    db.Add(post);

                    db.SaveChanges();

                    postId = post.Id;
                }
            };

            Because of = () => {
                using(var db = Db.CreateInMemory()) {
                    db.UpdateGraph(new Db.Post { Id = postId, Name = "Changed Bob" });

                    db.SaveChanges();
                }
                using(var db = Db.CreateInMemory()) _post = db.Posts.Single(p => p.Id == postId);
            };

            It Should_Update_Properties = () => _post.Name.ShouldEqual("Changed Bob");
        }

        #endregion

        //public class When_updating_a_new_entity
        //{
        //    static int postId;
        //    static Db.Post _post;

        //    Establish context = () => {
        //        using (var db = Db.CreateInMemory())
        //        {
        //            db.UpdateGraph(new Db.Post { Name = "New Bob" });

        //            db.SaveChanges();
        //        }
        //    };

        //    Because of = () => {
        //        using (var db = Db.CreateInMemory()) _post = db.Posts.Single(p => p.Id == postId);
        //    };

        //    It Should_Update_Properties = () => _post.Name.ShouldEqual("Changed Bob");
        //}
    }
}