using System.IO;

using SystemdManager.Parser;

using Xunit;

namespace SystemdManager.Tests;

public class ServiceStatusTests
{

    public const string Service1 = @"● rocketleague-api.service - ASP.NET Web API Process for RocketLeague-API.com
        Loaded: loaded(/etc/systemd/system/rocketleague-api.service; enabled; vendor preset: enabled)
        Active: active(running) since Fri 2022-03-25 09:30:02 UTC; 1 weeks 5 days ago
        Main PID: 655 (RocketLeague-AP)
        Tasks: 20 (limit: 4556)
            Memory: 538.0M
        CGroup: /system.slice/rocketleague-api.service
        └─655 /root/rocketleague-api/RocketLeague-API";

    public const string Service2 = @"● cdn.service - Cdn
         Loaded: loaded (/etc/systemd/system/cdn.service; enabled; vendor preset: enabled)
         Active: active (running) since Sat 2022-03-26 13:51:28 UTC; 1 weeks 4 days ago
       Main PID: 58697 (Cdn)
          Tasks: 14 (limit: 4556)
         Memory: 38.1M
         CGroup: /system.slice/cdn.service
                 └─58697 /root/cdn/Cdn";

    public const string Service3 = @"● cdn.service - Cdn
         Loaded: loaded (/etc/systemd/system/cdn.service; enabled; vendor preset: enabled)
         Active: inactive (dead) since Wed 2022-04-06 15:16:34 UTC; 825ms ago
        Process: 58697 ExecStart=/root/cdn/Cdn (code=exited, status=0/SUCCESS)
       Main PID: 58697 (code=exited, status=0/SUCCESS)";

    [Theory]
    [InlineData(Service1, true, 655, "538.0M", 2022, 3, 25, 9, 30, 2)]
    [InlineData(Service2, true, 58697, "38.1M", 2022, 3, 26, 13, 51, 28)]
    [InlineData(Service3, false, 0, "", 0, 0, 0, 0, 0, 0)]
    public void FirstServiceTest2(
        string content, bool isActive, int pid, string memoryUsage, 
        int year, int month, int day, int hour, int minute, int second)
    {
        var status = new ServiceStatus(content);

        Assert.Equal(isActive, status.IsActive);
        if (isActive)
        {
            Assert.Equal(pid, status.MainPid);
            Assert.Equal(memoryUsage, status.MemoryUsage);

            Assert.Equal(year, status.RunningSince.Year);
            Assert.Equal(month, status.RunningSince.Month);
            Assert.Equal(day, status.RunningSince.Day);

            Assert.Equal(hour, status.RunningSince.Hour);
            Assert.Equal(minute, status.RunningSince.Minute);
            Assert.Equal(second, status.RunningSince.Second);
        }
    }

}
