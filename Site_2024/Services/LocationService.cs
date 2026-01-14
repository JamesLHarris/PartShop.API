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
        public Location GetLocationBySiteId(int id)
        {
            string procName = "[dbo].[LocationHierarchy_GetBySiteId]";
            Location location = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@siteId", id);
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
        public List<Location> GetHierarchy(int siteId)
        {
            string procName = "[dbo].[LocationHierarchy_GetBySiteId]"; 
            List<Location> list = new List<Location>();

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@siteId", siteId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Location lh = MapSingleLocationHierarchy(reader, ref i);
                    list.Add(lh);
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
            location.Shelf = new Shelf();
            location.Section = new Section();
            location.Box = new Box();

            location.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Name = reader.GetSafeString(startingIndex++);
            location.Area.Id = reader.GetSafeInt32(startingIndex++);
            location.Area.Name = reader.GetSafeString(startingIndex++);
            location.Aisle.Id = reader.GetSafeInt32(startingIndex++);
            location.Aisle.Name = reader.GetSafeString(startingIndex++);
            location.Shelf.Id = reader.GetSafeInt32(startingIndex++);
            location.Shelf.Name = reader.GetSafeString(startingIndex++);
            location.Section.Id = reader.GetSafeInt32(startingIndex++);
            location.Section.Name = reader.GetSafeString(startingIndex++);
            location.Box.Id = reader.GetSafeInt32(startingIndex++);
            location.Box.Name = reader.GetSafeString(startingIndex++);
            return location;
        }

        private Location MapSingleLocationHierarchy(IDataReader reader, ref int startingIndex)
        {
            Location location = new Location();
            location.Site = new Models.Site();
            location.Area = new Area();
            location.Aisle = new Aisle();
            location.Shelf = new Shelf();
            location.Section = new Section();
            location.Box = new Box();

            location.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Id = reader.GetSafeInt32(startingIndex++);
            location.Site.Name = reader.GetSafeString(startingIndex++);
            location.Area.Id = reader.GetSafeInt32(startingIndex++);
            location.Area.Name = reader.GetSafeString(startingIndex++);
            location.Aisle.Id = reader.GetSafeInt32(startingIndex++);
            location.Aisle.Name = reader.GetSafeString(startingIndex++);
            location.Shelf.Id = reader.GetSafeInt32(startingIndex++);
            location.Shelf.Name = reader.GetSafeString(startingIndex++);
            location.Section.Id = reader.GetSafeInt32(startingIndex++);
            location.Section.Name = reader.GetSafeString(startingIndex++);
            location.Box.Id = reader.GetSafeInt32(startingIndex++);
            location.Box.Name = reader.GetSafeString(startingIndex++);
            return location;
        }

        #endregion

    }
}
