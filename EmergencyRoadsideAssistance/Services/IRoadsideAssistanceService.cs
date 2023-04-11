using EmergencyRoadsideAssistance.Models;
using System.Collections.Generic;

namespace EmergencyRoadsideAssistance.Services
{
	public interface IRoadsideAssistanceService
	{
		/**
		* This method is used to update the location of the roadside assistance service provider.
		*
		* @param assistant represents the roadside assistance service provider
		* @param assistantLocation represents the location of the roadside assistant
		*/
		Task UpdateAssistantLocation(Assistant assistant, Geolocation assistantLocation);
		/**
		* This method returns a collection of roadside assistants ordered by their distance from the input geo location.
		*
		* @param geolocation - geolocation from which to search for assistants
		* @param limit - the number of assistants to return
		* @return a sorted collection of assistants ordered ascending by distance from geoLocation
		*/
		Task<SortedSet<Assistant>> FindNearestAssistants(Geolocation geolocation, int limit);
		/**
		*
		* @param customerLocation - Location of the customer
		* @return The Assistant that is on their way to help
		*/
		Task<Assistant?> ReserveAssistant(Customer customer, Geolocation customerLocation);
		/**
		* This method releases an assistant either after they have completed work, or the customer no longer needs help.
		*
		* @param assistant - An assistant that was previously reserved by the customer
		*/
		Task ReleaseAssistant(Customer customer, Assistant assistant);
	}
}
