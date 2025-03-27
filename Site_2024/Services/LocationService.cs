using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Linq;
using Site_2024.Web.Api.Extensions;
using System.Data.SqlClient;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Requests;
using System.Security.Policy;

namespace Site_2024.Web.Api.Services
{
    public class LocationService : ILocationService
    {
        private IDataProvider _data = null;

        public LocationService(IDataProvider data)
        {
            _data = data;
        }

        #region ---GET---

        public Location GetLocationById(int id)
        {
            string procName = "[dbo].[Location_GetById]";
            Location location = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    location = MapSingleLocation(reader, ref startingIndex);
                });
            return location;
        }

        public List<Location> GetLocationsAll()
        {
            string procName = "[dbo].[Location_GetAll]";

            List<Location> list = new List<Location>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Location location = MapSingleLocation(reader, ref i);
                    list.Add(location);
                });

            return list;
        }
        public List<Models.Site> GetSitesAll()
        {
            string procName = "[dbo].[Site_GetAll]";

            List<Models.Site> list = new List<Models.Site>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Models.Site site = MapSingleSite(reader, ref i);
                    list.Add(site);
                });

            return list;
        }
        public List<Area> GetAreasAll()
        {
            string procName = "[dbo].[Area_GetAll]";

            List<Area> list = new List<Area>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Area area = MapSingleArea(reader, ref i);
                    list.Add(area);
                });

            return list;
        }
        public List<Aisle> GetAisleAll()
        {
            string procName = "[dbo].[Aisle_GetAll]";

            List<Aisle> list = new List<Aisle>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Aisle aisle = MapSingleAisle(reader, ref i);
                    list.Add(aisle);
                });

            return list;
        }
        public List<Section> GetSectionAll()
        {
            string procName = "[dbo].[Section_GetAll]";

            List<Section> list = new List<Section>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Section section = MapSingleSection(reader, ref i);
                    list.Add(section);
                });

            return list;
        }
        public List<Box> GetBoxAll()
        {
            string procName = "[dbo].[Box_GetAll]";

            List<Box> list = new List<Box>();

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    // No input parameters, but can be used if needed in the future
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Box box = MapSingleBox(reader, ref i);
                    list.Add(box);
                });

            return list;
        }

        #endregion

        #region ---POST&PUT---

        public int AddLocation(LocationAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Location_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonLocationParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }

        public int AddSite(SiteAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Site_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }
        public int AddArea(AreaAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Area_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }
        public int AddAisle(AisleAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Aisle_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }
        public int AddSection(SectionAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Section_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }
        public int AddBox(BoxAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Box_Insert]";

            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Name", model.Name);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }
            , returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }
        public void UpdateLocation(LocationUpdateRequest model)
        {
            string procName = "[dbo].[Location_Update]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonLocationParams(model, col);
                col.AddWithValue("@Id", model.Id);
            }
            , returnParameters: null);
        }

        #endregion

        #region ---DELETE---
        public void DeleteLocation(int id)
        {
            string procName = "[dbo].[Location_Delete]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            }
            , returnParameters: null);
        }
        #endregion

        #region ---MAPPER---

        private static void AddCommonLocationParams(LocationAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@siteId", model.SiteId);
            col.AddWithValue("@areaId", model.AreaId);
            col.AddWithValue("@aisleId", model.AisleId);
            col.AddWithValue("@sectionId", model.SectionId);
            col.AddWithValue("@boxId", model.BoxId);
        }

        private Location MapSingleLocation(IDataReader reader, ref int startingIndex)
        {
            Location location = new Location();
            location.Site = new Models.Site();
            location.Area = new Area();
            location.Aisle = new Aisle();
            location.Section = new Section();
            location.Box = new Box();

            location.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Name = reader.GetSafeString(startingIndex++);
            location.Area.Id = reader.GetSafeInt32(startingIndex++);
            location.Area.Name = reader.GetSafeString(startingIndex++);
            location.Aisle.Id = reader.GetSafeInt32(startingIndex++);
            location.Aisle.Name = reader.GetSafeString(startingIndex++);
            location.Section.Id = reader.GetSafeInt32(startingIndex++);
            location.Section.Name = reader.GetSafeString(startingIndex++);
            location.Box.Id = reader.GetSafeInt32(startingIndex++);
            location.Box.Name = reader.GetSafeString(startingIndex++);
            return location;
        }

        private Models.Site MapSingleSite(IDataReader reader, ref int startingIndex)
        {
            Models.Site site = new Models.Site();

            site.Id = reader.GetSafeInt32(startingIndex++);
            site.Name = reader.GetSafeString(startingIndex++);

            return site;
        }
        private Area MapSingleArea(IDataReader reader, ref int startingIndex)
        {
            Area area = new Area();

            area.Id = reader.GetSafeInt32(startingIndex++);
            area.Name = reader.GetSafeString(startingIndex++);

            return area;
        }
        private Aisle MapSingleAisle(IDataReader reader, ref int startingIndex)
        {
            Aisle aisle = new Aisle();

            aisle.Id = reader.GetSafeInt32(startingIndex++);
            aisle.Name = reader.GetSafeString(startingIndex++);

            return aisle;
        }
        private Section MapSingleSection(IDataReader reader, ref int startingIndex)
        {
            Section section = new Section();

            section.Id = reader.GetSafeInt32(startingIndex++);
            section.Name = reader.GetSafeString(startingIndex++);

            return section;
        }
        private Box MapSingleBox(IDataReader reader, ref int startingIndex)
        {
            Box box = new Box();

            box.Id = reader.GetSafeInt32(startingIndex++);
            box.Name = reader.GetSafeString(startingIndex++);

            return box;
        }

        public int AddSites(SiteAddRequest model)
        {
            throw new NotImplementedException();
        }

        public List<Models.Site> GetSiteAll()
        {
            throw new NotImplementedException();
        }

        public List<Area> GetAreaAll()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
