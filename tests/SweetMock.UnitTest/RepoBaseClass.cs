namespace SweetMock.UnitTest;

public class RepoBaseClass
{
    /// <summary>
    /// <see cref="SweetMock.UnitTest.RepoBaseClass(string)"/>
    /// </summary>
    public RepoBaseClass(string name)
    {
        
    }
    
    public virtual string Test(int test) => test.ToString();
}