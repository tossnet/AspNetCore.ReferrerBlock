namespace ReferrerBlock.Configuration;

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
        "crmlogichub",
        "ctysss",
        "followpathcrm",
        "hiplay704",
        "ikancupang",
        "jbuif",
        "karyapemuda",
        "kawruhbasa",
        "kaysdogs",
        "missywilkinson",
        "parakerja",
        "pvdwatersports",
        "pulbarholdings",
        "raymandservice",
        "rororush",
        "sscgdpracticeset",
        "sharpdrivers",
        "teknovidia",
        "tempatbelajar",
        "toworld123",
    };

    // List of blocked complete domains
    public HashSet<string> BlockedDomains { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "airplanesupdate.com",
        "aksarabrita.com",
        "barondamaluku.com",
        "biayakuliah.id",
        "brandipphoto.com",
        "chfaloans.com",
        "cozynestplans.com",
        "def-atk.com",
        "genshindb.org",
        "gazdp.com",
        "hellyeahomeland.com",
        "globalinsuranceresourcecenter.com",
        "indonesiamapan.com",
        "kebumenupdate.com",
        "kiprahkita.com",
        "liveasiannews.com",
        "pengkicau.com",
        "periodcostume.top",
        "mojok.co",
        "patioinstallationcompanies.com",
        "secondechance.org",
        "teknowarta.com",
        "tugassains.com",
        "wagnerdom.local",
        "wartaupdate.com",
        "wellsfederal",
        "xlyanghub.com",
    };
}
