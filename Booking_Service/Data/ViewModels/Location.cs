public class Province
{
    public string label { get; set; }
    public string value { get; set; }
    public District[] districts { get; set; }
}

public class District
{
    public string label { get; set; }
    public string value { get; set; }
    public Ward[] wards { get; set; }
}

public class Ward
{
    public string label { get; set; }
    public string value { get; set; }
}
