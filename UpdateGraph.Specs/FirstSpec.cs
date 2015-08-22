using Machine.Specifications;
using Machine.Specifications.Model;

namespace UpdateGraph.Specs
{
    public class FirstSpec
    {
        static string _temp;
        Because of = () => _temp = "Hello";

        It should_work = () => _temp.ShouldEqual("Hello");
    }
}