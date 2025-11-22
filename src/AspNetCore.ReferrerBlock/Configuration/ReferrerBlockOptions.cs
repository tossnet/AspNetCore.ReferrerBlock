namespace AspNetCore.ReferrerBlock.Configuration;

public class ReferrerBlockOptions
{
    // List of blocked domain extensions
    public HashSet<string> BlockedTLDs { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".biz.id",
        ".co.id",
        ".icu",
        ".in",
        ".xyz",
    };
    // List of suspicious domain patterns (incomplete or generic domains)
    public HashSet<string> BlockedPatterns { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "ctysss",
        "cekitchenware",
        "ctysss",
        "hiplay704",
        "ikancupang",
        "jbuif",
        "karyapemuda",
        "kawruhbasa",
        "missywilkinson",
        "parakerja",
        "pvdwatersports",
        "pulbarholdings",
        "raymandservice",
        "sscgdpracticeset",
        "sharpdrivers",
        "teknovidia",
        "tempatbelajar",
        "toworld123",
    };

    // List of blocked complete domains
    public HashSet<string> BlockedDomains { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "aksarabrita.com",
        "barondamaluku.com",
        "cozynestplans.com",
        "gazdp.com",
        "kebumenupdate.com",
        "kiprahkita.com",
        "pengkicau.com",
        "mojok.co",
        "patioinstallationcompanies.com",
        "tugassains.com",
        "wartaupdate.com",
        "wellsfederal",
        "xlyanghub.com",
    };
}
