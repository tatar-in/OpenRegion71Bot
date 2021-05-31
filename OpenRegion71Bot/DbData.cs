using System;
using System.Collections.Generic;

namespace OpenRegion71Bot
{
    class DbData
    {
        public class Problem
        {
            public int Id { get; set; }
            public int? CategoryId { get; set; }
            public Category Category { get; set; }
            public int? ThemeId { get; set; }
            public Theme Theme { get; set; }
            public string Adress { get; set; }
            public int? SourceId { get; set; }
            public Source Source { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? AnswerDate { get; set; }
            public string ProblemText { get; set; }
            public string ProblemPhotos { get; set; }
            public int StatusId { get; set; }
            public Status Status { get; set; }
            public string AnswerText { get; set; }
            public string AnswerPhotos { get; set; }
            public string IspolnitelName { get; set; }
            public int? IspolnitelId { get; set; }
            public Executor Ispolnitel { get; set; }
            public string DistrictId { get; set; }
            public District District { get; set; }
            public int? ChildID { get; set; }
            public int? ParentID { get; set; }
            public override bool Equals(Object obj)
            {
                if (obj is Problem ob) return Id.Equals(ob.Id) && CategoryId.Equals(ob.CategoryId) && ThemeId.Equals(ob.ThemeId) && Adress.Equals(ob.Adress)
                    && SourceId.Equals(ob.SourceId) && CreateDate.Equals(ob.CreateDate) && AnswerDate.Equals(ob.AnswerDate) 
                    && ProblemText.Equals(ob.ProblemText) && ProblemPhotos.Equals(ob.ProblemPhotos) && AnswerText.Equals(ob.AnswerText)
                    && AnswerPhotos.Equals(ob.AnswerPhotos) && StatusId.Equals(ob.StatusId) && DistrictId.Equals(ob.DistrictId) && ChildID.Equals(ob.ChildID)
                    && ParentID.Equals(ob.ParentID) && IspolnitelName.Equals(ob.IspolnitelName) && IspolnitelId.Equals(ob.IspolnitelId);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, CreateDate).GetHashCode();
            }
        }
        public class Theme
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Activity { get; set; }
            public int CategoryId { get; set; }
            public Category Category { get; set; }
            public List<Problem> Problems { get; set; }
            public Theme()
            {
                Problems = new List<Problem>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is Theme ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Activity.Equals(ob.Activity) && CategoryId.Equals(ob.CategoryId);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Activity { get; set; }
            public List<Theme> Themes { get; set; }
            public List<Problem> Problems { get; set; }
            public Category()
            {
                Themes = new List<Theme>();
                Problems = new List<Problem>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is Category ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Activity.Equals(ob.Activity) && Themes.Equals(ob.Themes);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class Status
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Problem> Problems { get; set; }
            public Status()
            {
                Problems = new List<Problem>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is Status ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class District
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<Problem> Problems { get; set; }
            public District()
            {
                Problems = new List<Problem>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is District ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class Executor
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Login { get; set; }
            public int StructureId { get; set; }
            public bool Activity { get; set; }
            public override bool Equals(Object obj)
            {
                if (obj is Executor ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Login.Equals(ob.Login) 
                        && StructureId.Equals(ob.StructureId) && Activity.Equals(ob.Activity);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CategoryId { get; set; }
            public SourceCategory Category { get; set; }
            public List<Problem> Problems { get; set; }
            public Source()
            {
                Problems = new List<Problem>(); 
            }
            public override bool Equals(Object obj)
            {
                if (obj is Source ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && CategoryId.Equals(ob.CategoryId);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class SourceCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Source> Sources { get; set; }
            public SourceCategory()
            {
                Sources = new List<Source>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is SourceCategory ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Sources.Equals(ob.Sources);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class Rule
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<User> Users { get; set; }
            public Rule()
            {
                Users = new List<User>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is Rule ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Users.Equals(ob.Users);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Nick { get; set; }
            public bool IsBot { get; set; }
            public List<Rule> Rules { get; set; }
            public User()
            {
                Rules = new List<Rule>();
            }
            public override bool Equals(Object obj)
            {
                if (obj is User ob) return Id.Equals(ob.Id) && Name.Equals(ob.Name) && Nick.Equals(ob.Nick) 
                        && IsBot.Equals(ob.IsBot);
                return false;
            }
            public override int GetHashCode()
            {
                return Tuple.Create(Id, Name).GetHashCode();
            }
        }
    }
}
