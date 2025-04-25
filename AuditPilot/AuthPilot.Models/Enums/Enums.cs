using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthPilot.Models.Enums
{
    public enum CompanyType
    {
        PrivateLable= 1,
        PublicComp=2,
        ForeignCompanies=3,
        PartnershipFirms=4,
        NonProfitOrganizations = 5,
        NBFC=6,
        PICS=7,
        ProvidentGratuityFunds=8,
        IndividualsSoleProprietors=9,
        Others=10
    }
    public enum ProjectType
    {
        Tax = 1,
        Audit = 2,
        Corporate = 3,
        Advisory = 4,
        ERP = 5,
        Bookkeeping = 6
    }
}
