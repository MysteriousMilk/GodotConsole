using GdUnit4;

using static GdUnit4.Assertions;

namespace Godot.Console.Tests;

[TestSuite]
public class ConsoleUnitTests
{
    [Before]
    public void BeforeTestSuite()
    {
        TestUtils.SetupLogAndConsole();
    }

    [AfterTest]
    public void AfterTestCase()
    {
        GodotConsole.RemoveAllCommands();
    }

    [TestCase]
    public void RegisterAndGetCommands()
    {
        // registers some commands
        GodotConsole.RegisterCommand("myCommand", (cmd, args) => { });
        GodotConsole.RegisterVariable("testDouble", 0.2);
        GodotConsole.RegisterVariable("testFloat", 0.2f);

        var cmdList = GodotConsole.GetCommandStringList();
        AssertThat(cmdList).HasSize(3);
        AssertThat(cmdList).Contains("myCommand".ToLower());
        AssertThat(cmdList).Contains("testDouble".ToLower());
        AssertThat(cmdList).Contains("testFloat".ToLower());
    }

    [TestCase]
    public void RegisterAndUpdateVar()
    {
        // registers some vars
        GodotConsole.RegisterVariable("testDouble", 0.2);
        GodotConsole.RegisterVariable("testFloat", 0.2f);
        GodotConsole.RegisterVariable("testInt", 2);
        GodotConsole.RegisterVariable("testBool", false);

        // update the vars
        GodotConsole.UpdateVariable("testDouble", 0.3);
        GodotConsole.UpdateVariable("testFloat", 0.3f);
        GodotConsole.UpdateVariable("testInt", 3);
        GodotConsole.UpdateVariable("testBool", true);

        // verify
        AssertThat(GodotConsole.GetVariable<double>("testDouble").Value).IsEqual(0.3);
        AssertThat(GodotConsole.GetVariable<float>("testFloat").Value).IsEqual(0.3f);
        AssertThat(GodotConsole.GetVariable<int>("testInt").Value).IsEqual(3);
        AssertThat(GodotConsole.GetVariable<bool>("testBool").Value).IsEqual(true);
    }

    [TestCase]
    public void ParseAndInvokeCommand()
    {
        bool testCommandInvoked = false;
        int argCount = 0;
        int arg1 = 0;
        double arg2 = 0.0;
        Vector2I arg3 = Vector2I.Zero;

        GodotConsole.RegisterCommand("testCommand", (cmd, args) =>
        {
            argCount = args.Length;
            testCommandInvoked = true;

            if (args.Length >= 3)
            {
                arg1 = args[0].AsInt32();
                arg2 = args[1].AsDouble();
                arg3 = args[2].AsVector2I();
            }
        });

        GodotConsole.ParseCommand("testCommand 1 2.0 8,3");

        AssertThat(testCommandInvoked).IsTrue();
        AssertThat(1).IsEqual(arg1);
        AssertThat(2.0).IsEqual(arg2);
        AssertThat(new Vector2I(8, 3)).IsEqual(arg3);

        GodotConsole.RegisterVariable("c_isFullscreen", true);
        GodotConsole.ParseCommand("c_isFullscreen False");

        var cvar = GodotConsole.GetVariable("c_isFullscreen");
        AssertThat(cvar.GetValue().AsBool()).IsFalse();
    }

    [TestCase]
    public void RetrieveVariableValue()
    {
        GodotConsole.RegisterVariable("testDouble", 0.2);
        GodotConsole.RegisterVariable("testString", "Some string");
        GodotConsole.RegisterVariable("testVec", new Vector3(0.1f, 0.2f, 0.3f));

        double val1 = GodotConsole.GetVariableValue<double>("testDouble");
        string val2 = GodotConsole.GetVariableValue<string>("testString");
        Vector3 val3 = GodotConsole.GetVariableValue<Vector3>("testVec");

        AssertThat(val1).IsEqual(0.2);
        AssertThat(val2).IsEqual("Some string");
        AssertThat(val3).IsEqual(new Vector3(0.1f, 0.2f, 0.3f));
    }

    [TestCase]
    public void VariantStringConversion()
    {
        Variant v1 = VariantHelper.ToVariant("5");
        AssertThat(v1.AsInt32()).IsEqual(5);

        Variant v2 = VariantHelper.ToVariant("234.837");
        AssertThat(v2.AsDouble()).IsEqual(234.837);

        Variant v3 = VariantHelper.ToVariant("1,2");
        AssertThat(v3.AsVector2I()).IsEqual(new Vector2I(1, 2));

        Variant v4 = VariantHelper.ToVariant("1,2,3");
        AssertThat(v4.AsVector3I()).IsEqual(new Vector3I(1, 2, 3));

        Variant v5 = VariantHelper.ToVariant("1.2,2.6");
        AssertThat(v5.AsVector2()).IsEqual(new Vector2(1.2f, 2.6f));

        Variant v6 = VariantHelper.ToVariant("1.2,2.6,4.3");
        AssertThat(v6.AsVector3()).IsEqual(new Vector3(1.2f, 2.6f, 4.3f));

        Variant v7 = VariantHelper.ToVariant("This is some text.");
        AssertThat(v7.AsString()).Equals("This is some text.");

        // try some edge cases
        Variant v8 = VariantHelper.ToVariant("1,2,");
        AssertThat(v8.AsVector2I()).IsEqual(new Vector2I(1, 2));

        Variant v9 = VariantHelper.ToVariant("10,");
        AssertThat(v9.AsInt32()).IsEqual(10);
    }

    [TestCase]
    public void WriteAndLoadConfig()
    {
        GodotConsole.RegisterVariable("c_isFullscreen", true, "ClientSettings", "Display");
        GodotConsole.RegisterVariable("c_resolution", new Vector2I(1280, 720), "ClientSettings", "Display");

        string dir = Utils.CreateTempDir("configs");
        GodotConsole.WriteConfig("ClientSettings", dir);

        GodotConsole.UpdateVariable("c_isFullScreen", false);
        GodotConsole.UpdateVariable("c_resolution", new Vector2I(1, 1));

        // relaod the config.. should override the updates above
        GodotConsole.LoadConfig("ClientSettings", dir);

        var var1 = GodotConsole.GetVariable("c_isFullscreen");
        var var2 = GodotConsole.GetVariable("c_resolution");

        AssertThat(var1.GetValue().AsBool()).IsTrue();
        AssertThat(var2.GetValue().AsVector2I()).IsEqual(new Vector2I(1280, 720));
    }

    [After]
    public void AfterTestSuite()
    {
        GodotConsole.RemoveAllCommands();
    }
}
