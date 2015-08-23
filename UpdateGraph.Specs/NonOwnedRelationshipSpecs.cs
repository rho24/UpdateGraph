using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace UpdateGraph.Specs
{
    public class NonOwnedRelationshipSpecs
    {
        Establish context = () => Db.ClearDb();

        #region Nested type: Db

        public class Db : DbContext
        {
            public DbSet<Post> Posts { get; set; }
            public DbSet<Blogger> Bloggers { get; set; }

            public Db(DbContextOptions<Db> options)
                : base(options) {}

            public static void ClearDb() {
                using(var db = CreateInMemory()) db.Database.EnsureDeleted();
            }

            public static Db CreateInMemory() {
                var builder = new DbContextOptionsBuilder<Db>();
                builder.UseInMemoryDatabase(persist: true);
                var options = builder.Options;

                var db = new Db(options);

                return db;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
            }

            #region Nested type: Blogger

            public class Blogger
            {
                public int Id { get; set; }

                public string Name { get; set; }

                public ICollection<Post> Posts { get; set; }
            }

            #endregion

            #region Nested type: Post

            public class Post
            {
                public int Id { get; set; }

                public string Name { get; set; }

                public Blogger Blogger { get; set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: When_updating_a_new_entity

        public class When_updating_a_new_entity
        {
            static Db.Post _post;

            static int postId;

            Establish context = () => {
                using(var db = Db.CreateInMemory()) {
                    var post = new Db.Post { Name = "New Bob" };
                    db.UpdateGraph(post);

                    db.SaveChanges();

                    postId = post.Id;
                }
            };

            Because of = () => { using(var db = Db.CreateInMemory()) _post = db.Posts.Single(p => p.Id == postId); };

            It Should_Update_Properties = () => _post.Name.ShouldEqual("New Bob");
        }

        #endregion

        #region Nested type: When_updating_an_existing_entity

        public class When_updating_an_existing_entity
        {
            static int postId;
            static Db.Post _post;

            Establish context = () => {
                using(var db = Db.CreateInMemory()) {
                    var blogger = new Db.Blogger { Name = "Bob" };
                    var post = new Db.Post() { Name = "Name", Blogger = blogger };
                    db.Add(blogger);
                    db.Add(post);

                    db.SaveChanges();

                    postId = post.Id;
                }
            };

            Because of = () => {
                using(var db = Db.CreateInMemory()) {
                    db.UpdateGraph(new Db.Post { Id = postId, Name = "Changed Name" });

                    db.SaveChanges();
                }
                using(var db = Db.CreateInMemory()) _post = db.Posts.Include(p => p.Blogger).Single(p => p.Id == postId);
            };

            It Should_not_change_the_relationship = () => _post.Blogger.Name.ShouldEqual("Bob");

            It Should_update_properties = () => _post.Name.ShouldEqual("Changed Name");
        }

        #endregion


        public class When_updating_a_relationship_with_tracked
        {
            static int postId;
            static Db.Post _post;

            Establish context = () => {
                using (var db = Db.CreateInMemory())
                {
                    var blogger = new Db.Blogger { Name = "Bob" };
                    var blogger2 = new Db.Blogger { Name = "Bob2" };
                    var post = new Db.Post() { Name = "Name", Blogger = blogger };
                    db.Add(blogger);
                    db.Add(blogger2);
                    db.Add(post);

                    db.SaveChanges();

                    postId = post.Id;
                }
            };

            Because of = () => {
                using (var db = Db.CreateInMemory())
                {
                    var blogger2 = db.Bloggers.Single(b => b.Name == "Bob2");
                    db.UpdateGraph(new Db.Post { Id = postId, Name = "Name", Blogger = blogger2 }, o => o.NonOwnedReference(p => p.Blogger));

                    db.SaveChanges();
                }
                using (var db = Db.CreateInMemory()) _post = db.Posts.Include(p => p.Blogger).Single(p => p.Id == postId);
            };

            It Should_change_the_relationship = () => _post.Blogger.Name.ShouldEqual("Bob2");

            It Should_not_update_properties = () => _post.Name.ShouldEqual("Name");
        }

        public class When_updating_a_relationship_without_tracked
        {
            static int postId;
            static int bloggerId;
            static Db.Post _post;

            Establish context = () => {
                using (var db = Db.CreateInMemory())
                {
                    var blogger = new Db.Blogger { Name = "Bob" };
                    var blogger2 = new Db.Blogger { Name = "Bob2" };
                    var post = new Db.Post() { Name = "Name", Blogger = blogger };
                    db.Add(blogger);
                    db.Add(blogger2);
                    db.Add(post);

                    db.SaveChanges();

                    postId = post.Id;
                    bloggerId = blogger2.Id;
                }
            };

            Because of = () => {
                using (var db = Db.CreateInMemory())
                {
                    db.UpdateGraph(new Db.Post { Id = postId, Name = "Name", Blogger = new Db.Blogger {Id = bloggerId} },
                        o => o.NonOwnedReference(p => p.Blogger));

                    db.SaveChanges();
                }
                using (var db = Db.CreateInMemory()) _post = db.Posts.Include(p => p.Blogger).Single(p => p.Id == postId);
            };

            It Should_change_the_relationship = () => _post.Blogger.Name.ShouldEqual("Bob2");

            It Should_not_update_properties = () => _post.Name.ShouldEqual("Name");
        }
    }
}