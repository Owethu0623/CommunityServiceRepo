namespace CommunityServiceProject.Migrations
{
    using CommunityServiceProject.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CommunityServiceProject.Models.Community>
    {
        public Configuration()
        {
            // Enable automatic migrations so pending model changes can be applied
            // automatically when Update-Database is run. Be cautious in production
            // environments — consider code-based migrations instead.
            AutomaticMigrationsEnabled = true;
            // Do not allow automatic data-loss causing changes by default.
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "CommunityServiceProject.Models.Community";
        }

      
               protected override void Seed(CommunityServiceProject.Models.Community context)
        {
            // Only create the default Administrator if one does not already exist
            if (!context.Administrators.Any(a =>
                a.EmailAddress == "admin@municipality.co.za"))
            {
                var administrator = new CommunityServiceProject.Models.Administrator
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailAddress = "admin@municipality.co.za",
                    PhoneNumber = "0123456789",
                    Password = CommunityServiceProject.Helpers.PasswordHelper.HashPassword("Admin123"),
                    AccountStatus = CommunityServiceProject.Models.AccountStatus.Active
                };

                context.Administrators.Add(administrator);
                context.SaveChanges();
            }

            context.FinanceOfficers.AddOrUpdate(
f => f.EmailAddress,
new FinanceOfficer
{
FirstName = "Municipal",
LastName = "Finance Officer",
EmailAddress = "finance@municipality.co.za",
Password = CommunityServiceProject.Helpers.PasswordHelper.HashPassword("Finance123"),
AccountStatus = AccountStatus.Active,
DateCreated = DateTime.Now
}
);

            // Seed a default HR Officer for development/testing so RBAC can be exercised
            context.HROfficers.AddOrUpdate(
                h => h.EmailAddress,
                new HROfficer
                {
                    FirstName = "Municipal",
                    LastName = "HR Officer",
                    EmailAddress = "hr@municipality.co.za",
                    Password = CommunityServiceProject.Helpers.PasswordHelper.HashPassword("HR123"),
                    AccountStatus = AccountStatus.Active,
                    DateCreated = DateTime.Now
                }
            );


            // ===========================================================
            // DEFAULT TECHNICIAN SKILLS
            // ===========================================================

            context.Skills.AddOrUpdate(
                s => s.SkillName,

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Plumbing",
                    Description = "Skills related to water supply, leaks, drainage and sewer maintenance."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Electrical",
                    Description = "Skills related to electrical systems, wiring and street lighting."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Road Maintenance",
                    Description = "Skills related to potholes, damaged roads and road surface repairs."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Waste Management",
                    Description = "Skills related to municipal waste, illegal dumping and overflowing bins."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Traffic Sign Maintenance",
                    Description = "Skills related to the repair and maintenance of damaged traffic signs."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "Sidewalk Maintenance",
                    Description = "Skills related to damaged sidewalks and pedestrian pathways."
                },

                new CommunityServiceProject.Models.Skill
                {
                    SkillName = "General Maintenance",
                    Description = "General municipal maintenance and repair skills."
                }
            );

            // ===========================================================
            // MUNICIPAL REFERENCE DATA SEED
            // ===========================================================


            // ============================================================
            // eTHEKWINI / DURBAN WARD SEED DATA
            // 2021 WARD DELIMITATION REFERENCE DATA
            // ============================================================

            var wardSeedData = new Dictionary<string, string>
{
    { "1", "Bhobhonono / Cato Ridge / Emvini / Nonoti" },
    { "2", "Qadi North / Maphephetha / Nyuswa / Ngcolosi" },
    { "3", "Inanda / Lindelani / Qadi / Shembe Village" },
    { "4", "Drummond / Inchanga / Hammarsdale / Fredville" },
    { "5", "Georgedale / Mophela / Sankontshe / Caluza" },
    { "6", "Mpumalanga A / Hammarsdale" },
    { "7", "Mpumalanga B / Hammarsdale" },
    { "8", "Hillcrest / Botha's Hill / Assagay" },
    { "9", "Kloof / Everton / Gillitts" },
    { "10", "Gillitts / Winston Park / Hillcrest Central" },
    { "11", "Embo / Waterfall / Crestholme" },
    { "12", "Shongweni / Cliffdale / Summerveld" },
    { "13", "Dassenhoek / Mariannhill / Tshelimnyama" },
    { "14", "Mariannhill / KwaNdengezi / Dassenhoek" },

    { "15", "Shallcross / Klaarwater / Savanna Park" },
    { "16", "Mariannhill / Chesterville Ext" },
    { "17", "Klaarwater / Welbedacht" },

    { "18", "Chatsworth - Bayview / Westcliff" },
    { "19", "Chatsworth - Umhlatuzana / Montford" },
    { "20", "Chatsworth - Westcliff / Woodhurst" },
    { "21", "Chatsworth - Kharwastan / Shallcross" },
    { "22", "Chatsworth - Arena Park / Croftdene" },
    { "23", "Lamontville / Chesterville / Umlazi Border" },

    { "24", "Chesterville / Bonela / Cato Manor" },
    { "25", "Umbilo / Glenwood / Bulwer" },
    { "26", "Durban CBD / Berea North / Essenwood" },
    { "27", "Berea / Musgrave / Overport" },
    { "28", "Morningside / Stamford Hill / Windermere" },
    { "29", "Sydenham / Overport / Springfield" },

    { "30", "Westville / Chiltern Hills / Dawncliffe" },
    { "31", "Westville / Beverley Hills / Westville North" },
    { "32", "Pinetown Central / Pinetown South" },
    { "33", "New Germany / Pinetown / Sarnia" },
    { "34", "Queensburgh / Malvern / Escombe" },

    { "36", "Wentworth / Austerville / Bluff" },
    { "37", "Merebank / Merewent / Austerville" },
    { "38", "Bluff / Grosvenor / Fynnland" },
    { "39", "Isipingo / Lotus Park / Orient Hills" },
    { "40", "Isipingo Beach / Malukazi / Isipingo Hills" },
    { "41", "Umlazi V / Umlazi CC" },
    { "42", "Umlazi D / Umlazi C" },
    { "43", "Umlazi Q / Umlazi Z" },
    { "44", "Umlazi J / Umlazi K" },
    { "45", "Umlazi L / Umlazi M" },
    { "46", "Umlazi N / Umlazi P" },
    { "47", "Umlazi H / Umlazi M" },
    { "48", "Umlazi U / Umlazi T" },
    { "49", "Umlazi G / Umlazi F" },
    { "50", "Umlazi A / Umlazi B" },
    { "51", "Folweni / Ezimbokodweni / Lotus Park" },
    { "52", "Amanzimtoti / Athlone Park / Kingsburgh" },
    { "53", "Doonside / Warner Beach / Illovo Beach" },
    { "54", "Winklespruit / Illovo / Karridene" },
    { "55", "Umkomaas / Widenham / Clansthal" },
    { "56", "Craigieburn / Umkomaas / Widenham" },
    { "57", "KwaMakhutha A" },
    { "58", "KwaMakhutha B" },
    { "59", "Adams Mission / Ezigojini / Amanzimtoti Rural" },
    { "60", "Umbumbulu / Ogunjini / Ezimbokodweni" },
    { "61", "Umbumbulu / Embo / Eshesheni" },
    { "62", "Embo / Malukazi / Umbumbulu" },

    { "63", "Clermont / KwaDabeka / Umkhumbane" },
    { "64", "KwaDabeka A" },
    { "65", "KwaDabeka B / Clermont" },

    { "66", "Newlands West A" },
    { "67", "Newlands West B / Bester" },
    { "68", "Newlands West C / Westrich" },
    { "69", "Newlands East / Parlock / Briardene" },
    { "70", "KwaMashu K / KwaMashu L" },
    { "71", "KwaMashu L / KwaMashu M" },
    { "72", "KwaMashu J / KwaMashu H" },
    { "73", "KwaMashu N / KwaMashu P" },
    { "74", "KwaMashu B / KwaMashu C" },
    { "75", "KwaMashu A / KwaMashu D" },
    { "76", "Ntuzuma A / Ntuzuma B" },
    { "77", "Ntuzuma / Lindelani / Richmond Farm" },
    { "78", "Ntuzuma E / Ntuzuma F / Ntuzuma G" },
    { "79", "Ntuzuma G / KwaNozaza / Lindelani" },
    { "80", "Inanda Glebe / Inanda Seminary" },
    { "81", "Inanda Ohlange / Inanda A / Inanda C" },
    { "82", "Inanda Congo / Inanda Newtown" },
    { "83", "Inanda Newtown A / Amatikwe" },
    { "84", "Inanda Newtown B / Inanda Bhambayi" },
    { "85", "Inanda White City / Inanda Ntanda" },
    { "86", "Inanda Amaoti Cuba / Inanda" },
    { "87", "Inanda Amaoti / Inanda" },
    { "88", "Phoenix Grove End / Foresthaven" },
    { "89", "Phoenix Stanmore / Grove End" },
    { "90", "Phoenix Stonebridge / Eastbury" },
    { "91", "Phoenix Eastbury / Stanmore" },
    { "92", "Phoenix Clayfield / Longcroft" },
    { "93", "Phoenix Longcroft / Rydalvale" },
    { "94", "Phoenix Rydalvale / Woodview" },
    { "95", "Phoenix Woodview / Brookdale" },
    { "96", "Phoenix Caneside / Unit 17" },
    { "97", "Verulam / Brindhaven / Cordoba Gardens" },
    { "98", "Verulam Central / Cottonlands / Mountview" },
    { "99", "Cornubia / Waterloo / Ottawa" },
    { "100", "Waterloo / Umdloti / La Mercy" },
    { "101", "Umhlanga / Sunningdale / Prestondale" },
    { "102", "Durban North / Virginia / Umhlanga Rocks" },

    { "103", "Durban North / Umhlanga / La Lucia" },
    { "104", "Reservoir Hills / Clare Hills / Bonela" },
    { "105", "Magabheni / Umnini / Amahlongwa" },
    { "106", "Umnini / Clansthal / Umnini North" },
    { "107", "Umkomaas Rural / Amahlongwa" },
    { "108", "Emona / Tongaat Central / Chelmsford Heights" },
    { "109", "Tongaat / Maidstone / Sandfields" },
    { "110", "Hambanathi / Westbrook / Tongaat Beach" },
    { "111", "La Mercy / Desainagar / Hazelmere" }
};

            foreach (var wardData in wardSeedData)
            {
                string wardNumber = wardData.Key;
                string wardName = wardData.Value;

                var existingWard = context.Wards
                    .FirstOrDefault(w => w.WardNumber == wardNumber);

                if (existingWard == null)
                {
                    context.Wards.Add(new CommunityServiceProject.Models.Ward
                    {
                        WardNumber = wardNumber,
                        WardName = wardName,
                        Description = "eThekwini Municipality Ward " + wardNumber +
                                      " - " + wardName,
                        IsActive = true
                    });
                }
            }

            context.SaveChanges();


            // ============================================================
            // MUNICIPAL SERVICE TYPES
            // ============================================================

            context.ServiceTypes.AddOrUpdate(
                s => s.ServiceCode,

                new ServiceType
                {
                    ServiceCode = "HALL-BOOK",
                    ServiceName = "Community Hall Booking",
                    Description = "Request to book an eThekwini municipal community hall for meetings, functions, community events, training or other approved activities.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "VENUE-BOOK",
                    ServiceName = "Municipal Venue Booking",
                    Description = "Request to book an available municipal venue or facility for an approved private, community, cultural or public event.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "SPORT-BOOK",
                    ServiceName = "Sports Facility Booking",
                    Description = "Request to book a municipal sports facility, sports field or recreation facility for sporting activities, training or approved events.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "STADIUM-BOOK",
                    ServiceName = "Stadium Facility Booking",
                    Description = "Request to book an available municipal stadium facility, function area, field or related stadium space for an approved event or activity.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "POOL-BOOK",
                    ServiceName = "Swimming Pool Facility Booking",
                    Description = "Request to use or book an available municipal swimming pool facility for an approved recreational, training or organised activity.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "PARK-BOOK",
                    ServiceName = "Municipal Park Booking",
                    Description = "Request to use or book an available municipal park or recreational open space for an approved event or community activity.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "MARKET-BOOK",
                    ServiceName = "Municipal Market Facility Request",
                    Description = "Request to use an available municipal market facility, trading space or stall subject to municipal approval and applicable requirements.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "EVENT-FAC",
                    ServiceName = "Municipal Event Facility Request",
                    Description = "Request for use of an available municipal facility for a public, cultural, community, educational or recreational event.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "SPORT-FIELD",
                    ServiceName = "Sports Field Booking",
                    Description = "Request to book a municipal sports field for football, cricket, athletics, tennis, bowling or another approved sporting activity.",
                    IsChargeable = true,
                    IsActive = true
                },

                new ServiceType
                {
                    ServiceCode = "COMMUNITY-FAC",
                    ServiceName = "Community Facility Request",
                    Description = "Request to use an available municipal community facility for an approved community, educational, social or recreational activity.",
                    IsChargeable = true,
                    IsActive = true
                }
            );

            context.SaveChanges();


            // ===========================================================
            // 3. FIND EXISTING SYSTEM ADMINISTRATOR
            // ===========================================================
            // Uses your existing administrator seed.
            // We do NOT create or modify another administrator.

            var systemAdmin = context.Administrators
                .FirstOrDefault(a => a.EmailAddress == "admin@municipality.co.za");

            if (systemAdmin == null)
            {
                throw new Exception(
                    "System Administrator was not found. " +
                    "Expected administrator: admin@municipality.co.za");
            }


            // ===========================================================
            // 4. MUNICIPAL PROJECTS
            // ===========================================================

            var projectWard1 = context.Wards.First(w => w.WardNumber == "1");
            var projectWard2 = context.Wards.First(w => w.WardNumber == "2");
            var projectWard3 = context.Wards.First(w => w.WardNumber == "3");
            var projectWard4 = context.Wards.First(w => w.WardNumber == "4");
            var projectWard5 = context.Wards.First(w => w.WardNumber == "5");
            var projectWard6 = context.Wards.First(w => w.WardNumber == "6");


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-001"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-001",
                    ProjectName = "Municipal Road Rehabilitation Programme",
                    ProjectType = "Road Infrastructure",
                    ProjectScope = "Rehabilitation of selected municipal road sections and associated public road infrastructure.",
                    Description = "Planned rehabilitation of priority municipal road sections to improve road condition, safety and accessibility.",
                    ProjectLocation = "Durban metropolitan road network",
                    Latitude = -29.8587,
                    Longitude = 31.0218,
                    LocationDescription = "Selected municipal road sections within the metropolitan road network.",
                    WardID = projectWard1.WardID,
                    StartDate = new DateTime(2026, 10, 15),
                    ExpectedCompletionDate = new DateTime(2027, 6, 30),
                    Status = MunicipalProjectStatus.Planned,
                    Priority = MunicipalProjectPriority.High,
                    EstimatedBudget = 18500000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-002"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-002",
                    ProjectName = "Stormwater Drainage Improvement Project",
                    ProjectType = "Stormwater Infrastructure",
                    ProjectScope = "Improvement and rehabilitation of selected municipal stormwater drainage infrastructure.",
                    Description = "Improvement of stormwater drainage infrastructure to reduce flooding risks and improve water flow.",
                    ProjectLocation = "Umgeni area",
                    Latitude = -29.8026,
                    Longitude = 30.9950,
                    LocationDescription = "Stormwater drainage network serving the Umgeni area.",
                    WardID = projectWard2.WardID,
                    StartDate = new DateTime(2026, 11, 1),
                    ExpectedCompletionDate = new DateTime(2027, 5, 31),
                    Status = MunicipalProjectStatus.Planned,
                    Priority = MunicipalProjectPriority.Critical,
                    EstimatedBudget = 12500000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-003"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-003",
                    ProjectName = "Community Facility Upgrade Programme",
                    ProjectType = "Community Facility",
                    ProjectScope = "Upgrade of selected municipal community facilities including accessibility and public-use improvements.",
                    Description = "Upgrade programme aimed at improving the condition, accessibility and functionality of municipal community facilities.",
                    ProjectLocation = "Umlazi community facilities",
                    Latitude = -29.9697,
                    Longitude = 30.8837,
                    LocationDescription = "Selected community facilities serving residents in Umlazi.",
                    WardID = projectWard3.WardID,
                    StartDate = new DateTime(2026, 9, 15),
                    ExpectedCompletionDate = new DateTime(2027, 3, 31),
                    Status = MunicipalProjectStatus.InProgress,
                    Priority = MunicipalProjectPriority.Medium,
                    EstimatedBudget = 8200000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-004"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-004",
                    ProjectName = "Municipal Sports Facility Rehabilitation",
                    ProjectType = "Sports Facility",
                    ProjectScope = "Rehabilitation of municipal sports facilities and associated public recreation infrastructure.",
                    Description = "Rehabilitation of selected sports facilities to improve safety, usability and community access.",
                    ProjectLocation = "Chatsworth sports facilities",
                    Latitude = -29.9185,
                    Longitude = 30.8785,
                    LocationDescription = "Selected municipal sports facilities in the Chatsworth area.",
                    WardID = projectWard4.WardID,
                    StartDate = new DateTime(2026, 12, 1),
                    ExpectedCompletionDate = new DateTime(2027, 8, 31),
                    Status = MunicipalProjectStatus.Planned,
                    Priority = MunicipalProjectPriority.High,
                    EstimatedBudget = 9700000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-005"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-005",
                    ProjectName = "Public Park Improvement Project",
                    ProjectType = "Public Park",
                    ProjectScope = "Improvement of municipal public park infrastructure, landscaping and public amenities.",
                    Description = "Upgrade of selected public park facilities and amenities to improve recreational use.",
                    ProjectLocation = "Pinetown public park area",
                    Latitude = -29.8136,
                    Longitude = 30.8587,
                    LocationDescription = "Municipal public park infrastructure in the Pinetown area.",
                    WardID = projectWard5.WardID,
                    StartDate = new DateTime(2026, 10, 20),
                    ExpectedCompletionDate = new DateTime(2027, 4, 30),
                    Status = MunicipalProjectStatus.Planned,
                    Priority = MunicipalProjectPriority.Medium,
                    EstimatedBudget = 5400000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalProjects.Any(p =>
                p.ProjectCode == "MPRJ-2026-006"))
            {
                context.MunicipalProjects.Add(new MunicipalProject
                {
                    ProjectCode = "MPRJ-2026-006",
                    ProjectName = "Municipal Water Infrastructure Upgrade",
                    ProjectType = "Water Infrastructure",
                    ProjectScope = "Upgrade and rehabilitation of selected municipal water infrastructure.",
                    Description = "Improvement of water infrastructure to support reliable municipal water services.",
                    ProjectLocation = "KwaMashu water infrastructure network",
                    Latitude = -29.7367,
                    Longitude = 30.9558,
                    LocationDescription = "Selected municipal water infrastructure serving KwaMashu.",
                    WardID = projectWard6.WardID,
                    StartDate = new DateTime(2026, 11, 15),
                    ExpectedCompletionDate = new DateTime(2027, 9, 30),
                    Status = MunicipalProjectStatus.Planned,
                    Priority = MunicipalProjectPriority.Critical,
                    EstimatedBudget = 22000000m,
                    ResponsibleAdministratorID = systemAdmin.AdministratorID,
                    DateRegistered = DateTime.Now,
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }

            context.SaveChanges();


            // ===========================================================
            // 5. MUNICIPAL ASSETS
            // ===========================================================

            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-001"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-001",
                    AssetName = "Durban CBD Street Light 001",
                    AssetType = "Street Lighting",
                    AssetCategory = "Streetlight",
                    Description = "Municipal street lighting infrastructure serving the Durban CBD road network.",
                    Latitude = -29.8587,
                    Longitude = 31.0218,
                    LocationDescription = "Street lighting installation within the Durban CBD road network.",
                    WardID = projectWard1.WardID,
                    Condition = AssetCondition.Good,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-3),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-002"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-002",
                    AssetName = "Umgeni High-Mast Light 001",
                    AssetType = "Street Lighting",
                    AssetCategory = "High-Mast Light",
                    Description = "Municipal high-mast lighting infrastructure serving a public area in the Umgeni area.",
                    Latitude = -29.8026,
                    Longitude = 30.9950,
                    LocationDescription = "High-mast lighting installation serving the Umgeni area.",
                    WardID = projectWard2.WardID,
                    Condition = AssetCondition.Fair,
                    Status = AssetStatus.RequiresAttention,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-4),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-003"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-003",
                    AssetName = "Umlazi Water Pipe 001",
                    AssetType = "Water Infrastructure",
                    AssetCategory = "Water Pipe",
                    Description = "Municipal water distribution pipe forming part of the local water infrastructure network.",
                    Latitude = -29.9697,
                    Longitude = 30.8837,
                    LocationDescription = "Water distribution infrastructure serving the Umlazi area.",
                    WardID = projectWard3.WardID,
                    Condition = AssetCondition.Good,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-004"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-004",
                    AssetName = "Chatsworth Water Valve 001",
                    AssetType = "Water Infrastructure",
                    AssetCategory = "Water Valve",
                    Description = "Municipal water control valve installed as part of the local water distribution network.",
                    Latitude = -29.9185,
                    Longitude = 30.8785,
                    LocationDescription = "Water distribution control point in the Chatsworth area.",
                    WardID = projectWard4.WardID,
                    Condition = AssetCondition.Fair,
                    Status = AssetStatus.UnderMaintenance,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddDays(-10),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-005"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-005",
                    AssetName = "Pinetown Stormwater Drain 001",
                    AssetType = "Stormwater Infrastructure",
                    AssetCategory = "Stormwater Drain",
                    Description = "Municipal stormwater drainage infrastructure serving the local road and public drainage network.",
                    Latitude = -29.8136,
                    Longitude = 30.8587,
                    LocationDescription = "Stormwater drainage installation serving the Pinetown area.",
                    WardID = projectWard5.WardID,
                    Condition = AssetCondition.Poor,
                    Status = AssetStatus.RequiresAttention,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddDays(-20),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-5),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-006"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-006",
                    AssetName = "KwaMashu Road 001",
                    AssetType = "Road Infrastructure",
                    AssetCategory = "Road",
                    Description = "Municipal road infrastructure forming part of the local public road network.",
                    Latitude = -29.7367,
                    Longitude = 30.9558,
                    LocationDescription = "Municipal road section serving the KwaMashu area.",
                    WardID = projectWard6.WardID,
                    Condition = AssetCondition.Good,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-007"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-007",
                    AssetName = "Durban CBD Traffic Signal 001",
                    AssetType = "Traffic Infrastructure",
                    AssetCategory = "Traffic Signal",
                    Description = "Municipal traffic signal infrastructure supporting road intersection management.",
                    Latitude = -29.8600,
                    Longitude = 31.0250,
                    LocationDescription = "Traffic-controlled intersection within the Durban CBD.",
                    WardID = projectWard1.WardID,
                    Condition = AssetCondition.Excellent,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddDays(-15),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-008"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-008",
                    AssetName = "Umlazi Waste Container 001",
                    AssetType = "Waste Management",
                    AssetCategory = "Waste Container",
                    Description = "Municipal waste container forming part of public waste management infrastructure.",
                    Latitude = -29.9670,
                    Longitude = 30.8850,
                    LocationDescription = "Public waste management point in the Umlazi area.",
                    WardID = projectWard3.WardID,
                    Condition = AssetCondition.Fair,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-009"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-009",
                    AssetName = "Pinetown Community Facility 001",
                    AssetType = "Public Facilities",
                    AssetCategory = "Community Facility",
                    Description = "Municipal community facility supporting public services and community activities.",
                    Latitude = -29.8150,
                    Longitude = 30.8600,
                    LocationDescription = "Municipal community facility in the Pinetown area.",
                    WardID = projectWard5.WardID,
                    Condition = AssetCondition.Good,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-3),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-010"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-010",
                    AssetName = "Chatsworth Park Facility 001",
                    AssetType = "Parks and Recreation",
                    AssetCategory = "Park Facility",
                    Description = "Municipal park and recreation infrastructure serving the surrounding community.",
                    Latitude = -29.9200,
                    Longitude = 30.8800,
                    LocationDescription = "Municipal park facility in the Chatsworth area.",
                    WardID = projectWard4.WardID,
                    Condition = AssetCondition.Poor,
                    Status = AssetStatus.UnderMaintenance,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddDays(-25),
                    LastMaintenanceDate = DateTime.Now.AddDays(-5),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-011"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-011",
                    AssetName = "Durban Municipal Building 001",
                    AssetType = "Municipal Buildings",
                    AssetCategory = "Municipal Building",
                    Description = "Municipal building infrastructure supporting public administration and municipal operations.",
                    Latitude = -29.8570,
                    Longitude = 31.0240,
                    LocationDescription = "Municipal administrative building in the Durban CBD.",
                    WardID = projectWard1.WardID,
                    Condition = AssetCondition.Excellent,
                    Status = AssetStatus.Active,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddMonths(-1),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }


            if (!context.MunicipalAssets.Any(a =>
                a.AssetCode == "AST-2026-012"))
            {
                context.MunicipalAssets.Add(new MunicipalAsset
                {
                    AssetCode = "AST-2026-012",
                    AssetName = "Umgeni Drainage Channel 001",
                    AssetType = "Stormwater Infrastructure",
                    AssetCategory = "Drainage Channel",
                    Description = "Municipal drainage channel supporting stormwater management in the Umgeni area.",
                    Latitude = -29.8050,
                    Longitude = 30.9970,
                    LocationDescription = "Stormwater drainage channel serving the Umgeni area.",
                    WardID = projectWard2.WardID,
                    Condition = AssetCondition.Critical,
                    Status = AssetStatus.RequiresAttention,
                    DateRegistered = DateTime.Now,
                    LastInspectionDate = DateTime.Now.AddDays(-10),
                    LastMaintenanceDate = DateTime.Now.AddMonths(-6),
                    CreatedByAdministratorID = systemAdmin.AdministratorID
                });
            }

            context.SaveChanges();


            // ===========================================================
            // 6. TECHNICIAN OPPORTUNITIES
            // ===========================================================

            if (!context.TechnicianOpportunities.Any(o =>
                o.OpportunityCode == "TOP-2026-001"))
            {
                context.TechnicianOpportunities.Add(new TechnicianOpportunity
                {
                    OpportunityCode = "TOP-2026-001",
                    Title = "Municipal Electrical Maintenance Technician",

                    Description =
                        "Responsible for maintaining, inspecting and repairing municipal electrical infrastructure including street lighting, area lighting, high-mast lighting and associated electrical equipment.",

                    Responsibilities =
                        "Inspect municipal electrical infrastructure and identify faults.\n" +
                        "Repair damaged street and area lighting equipment.\n" +
                        "Perform preventative electrical maintenance.\n" +
                        "Diagnose electrical faults and equipment failures.\n" +
                        "Complete maintenance records and technical reports.\n" +
                        "Follow municipal safety procedures and electrical regulations.",

                    Requirements =
                        "Valid driver's licence.\n" +
                        "Ability to work independently and as part of a maintenance team.\n" +
                        "Good fault-finding and problem-solving ability.\n" +
                        "Ability to work in outdoor municipal environments.\n" +
                        "Willingness to work after-hours during emergency maintenance.",

                    RequiredQualifications =
                        "National Diploma or equivalent qualification in Electrical Engineering / Electrical Technology or a relevant electrical trade qualification.",

                    RequiredExperience =
                        "2–3 years of practical experience in electrical maintenance, preferably involving public or municipal infrastructure.",

                    ApplicationInstructions =
                        "Complete the online application form and provide accurate personal, qualification and experience information. Applicants must upload supporting qualification documentation where requested.",

                    ApplicationStartDate = new DateTime(2026, 10, 1),
                    ApplicationDeadline = new DateTime(2026, 10, 31),

                    Status = TechnicianOpportunityStatus.Published,
                    EmploymentType = "Permanent",
                    NumberOfPositions = 3,

                    DateCreated = new DateTime(2026, 9, 25),
                    CreatedByAdministratorID = systemAdmin.AdministratorID,
                    PublishedDate = new DateTime(2026, 10, 1)
                });
            }


            if (!context.TechnicianOpportunities.Any(o =>
                o.OpportunityCode == "TOP-2026-002"))
            {
                context.TechnicianOpportunities.Add(new TechnicianOpportunity
                {
                    OpportunityCode = "TOP-2026-002",
                    Title = "Municipal Plumbing Maintenance Technician",

                    Description =
                        "Responsible for investigating and repairing municipal plumbing, water-supply and sanitation infrastructure and responding to reported maintenance problems.",

                    Responsibilities =
                        "Inspect water and sanitation infrastructure.\n" +
                        "Repair leaks, damaged pipes, valves and related fittings.\n" +
                        "Investigate reported plumbing faults.\n" +
                        "Conduct preventative maintenance activities.\n" +
                        "Record completed maintenance work.\n" +
                        "Escalate major infrastructure defects where required.",

                    Requirements =
                        "Practical plumbing maintenance skills.\n" +
                        "Valid driver's licence.\n" +
                        "Ability to interpret basic technical information.\n" +
                        "Good communication and problem-solving skills.\n" +
                        "Ability to work outdoors and respond to service requests.",

                    RequiredQualifications =
                        "Relevant plumbing trade qualification or equivalent recognised technical qualification.",

                    RequiredExperience =
                        "2 years or more practical plumbing maintenance experience.",

                    ApplicationInstructions =
                        "Submit the online application with complete qualification and work-experience details. Ensure all required supporting documentation is uploaded before submitting the application.",

                    ApplicationStartDate = new DateTime(2026, 10, 5),
                    ApplicationDeadline = new DateTime(2026, 11, 5),

                    Status = TechnicianOpportunityStatus.Published,
                    EmploymentType = "Permanent",
                    NumberOfPositions = 4,

                    DateCreated = new DateTime(2026, 9, 29),
                    CreatedByAdministratorID = systemAdmin.AdministratorID,
                    PublishedDate = new DateTime(2026, 10, 5)
                });
            }


            if (!context.TechnicianOpportunities.Any(o =>
                o.OpportunityCode == "TOP-2026-003"))
            {
                context.TechnicianOpportunities.Add(new TechnicianOpportunity
                {
                    OpportunityCode = "TOP-2026-003",
                    Title = "Roads and Infrastructure Maintenance Technician",

                    Description =
                        "Responsible for technical maintenance activities involving municipal roads, potholes, sidewalks, road surfaces and related public infrastructure.",

                    Responsibilities =
                        "Inspect damaged roads and related infrastructure.\n" +
                        "Assess reported maintenance defects.\n" +
                        "Assist with road and sidewalk repair activities.\n" +
                        "Identify recurring infrastructure problems.\n" +
                        "Record materials and work performed.\n" +
                        "Support preventative maintenance programmes.",

                    Requirements =
                        "Knowledge of municipal road maintenance practices.\n" +
                        "Ability to inspect infrastructure and identify defects.\n" +
                        "Valid driver's licence.\n" +
                        "Ability to work in varying weather and field conditions.\n" +
                        "Good teamwork and reporting skills.",

                    RequiredQualifications =
                        "National Certificate, Diploma or equivalent qualification in Civil Engineering, Construction, Road Construction or a related field.",

                    RequiredExperience =
                        "2 years practical experience in civil works, road maintenance or infrastructure maintenance.",

                    ApplicationInstructions =
                        "Complete the online application and provide details of relevant technical qualifications and practical experience. Supporting documents must be submitted where required.",

                    ApplicationStartDate = new DateTime(2026, 10, 10),
                    ApplicationDeadline = new DateTime(2026, 11, 10),

                    Status = TechnicianOpportunityStatus.Published,
                    EmploymentType = "Contract",
                    NumberOfPositions = 5,

                    DateCreated = new DateTime(2026, 10, 2),
                    CreatedByAdministratorID = systemAdmin.AdministratorID,
                    PublishedDate = new DateTime(2026, 10, 10)
                });
            }


            if (!context.TechnicianOpportunities.Any(o =>
                o.OpportunityCode == "TOP-2026-004"))
            {
                context.TechnicianOpportunities.Add(new TechnicianOpportunity
                {
                    OpportunityCode = "TOP-2026-004",
                    Title = "Stormwater Infrastructure Maintenance Technician",

                    Description =
                        "Responsible for inspecting and maintaining municipal stormwater infrastructure, including drainage systems, culverts, channels and associated structures.",

                    Responsibilities =
                        "Inspect stormwater drainage infrastructure.\n" +
                        "Identify blocked, damaged or deteriorated infrastructure.\n" +
                        "Support maintenance and repair activities.\n" +
                        "Record infrastructure defects and completed work.\n" +
                        "Assist with preventative maintenance planning.\n" +
                        "Respond to urgent stormwater maintenance requirements.",

                    Requirements =
                        "Knowledge of drainage and stormwater infrastructure.\n" +
                        "Practical maintenance and inspection skills.\n" +
                        "Ability to interpret basic infrastructure drawings.\n" +
                        "Valid driver's licence.\n" +
                        "Ability to work safely in outdoor and potentially difficult site conditions.",

                    RequiredQualifications =
                        "Relevant qualification in Civil Engineering, Water Engineering, Construction or Infrastructure Maintenance.",

                    RequiredExperience =
                        "2 years experience in stormwater, drainage, civil works or related infrastructure maintenance.",

                    ApplicationInstructions =
                        "Applications for this opportunity must be submitted through the online recruitment system during the advertised application period.",

                    ApplicationStartDate = new DateTime(2026, 9, 1),
                    ApplicationDeadline = new DateTime(2026, 9, 30),

                    Status = TechnicianOpportunityStatus.Closed,
                    EmploymentType = "Fixed-Term",
                    NumberOfPositions = 2,

                    DateCreated = new DateTime(2026, 8, 25),
                    CreatedByAdministratorID = systemAdmin.AdministratorID,
                    PublishedDate = new DateTime(2026, 9, 1),
                    ClosedDate = new DateTime(2026, 9, 30)
                });
            }


            if (!context.TechnicianOpportunities.Any(o =>
                o.OpportunityCode == "TOP-2026-005"))
            {
                context.TechnicianOpportunities.Add(new TechnicianOpportunity
                {
                    OpportunityCode = "TOP-2026-005",
                    Title = "General Municipal Maintenance Technician",

                    Description =
                        "Supports municipal maintenance teams with general infrastructure inspections, minor repairs and maintenance activities across public facilities and municipal service areas.",

                    Responsibilities =
                        "Conduct basic infrastructure inspections.\n" +
                        "Assist skilled maintenance teams with repairs.\n" +
                        "Report damaged municipal infrastructure.\n" +
                        "Assist with preventative maintenance tasks.\n" +
                        "Maintain accurate work records.\n" +
                        "Follow occupational health and safety procedures.",

                    Requirements =
                        "Practical maintenance ability.\n" +
                        "Good communication skills.\n" +
                        "Ability to work as part of a technical team.\n" +
                        "Physically capable of performing field-based maintenance duties.\n" +
                        "Valid driver's licence is advantageous.",

                    RequiredQualifications =
                        "Relevant technical certificate, vocational qualification or recognised maintenance-related qualification.",

                    RequiredExperience =
                        "1–2 years experience in general maintenance, facilities maintenance or municipal infrastructure support.",

                    ApplicationInstructions =
                        "Applicants must complete the online application form and provide accurate qualification and experience information before submitting the application.",

                    ApplicationStartDate = new DateTime(2026, 10, 15),
                    ApplicationDeadline = new DateTime(2026, 11, 15),

                    Status = TechnicianOpportunityStatus.Published,
                    EmploymentType = "Temporary",
                    NumberOfPositions = 6,

                    DateCreated = new DateTime(2026, 10, 5),
                    CreatedByAdministratorID = systemAdmin.AdministratorID,
                    PublishedDate = new DateTime(2026, 10, 5)
                });
            }

            context.SaveChanges();
        }


    }
}