using FollowMe.Controllers;
using FollowMe.Data;

namespace FollowMe.Extention
{
    public static class ReflectionExtensions
    {
        public static void SetCars(List<Car> newCars)
        {
            var field = typeof(FollowMeController)
                .GetField("cars", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            field?.SetValue(null, newCars);
        }
    }
}
