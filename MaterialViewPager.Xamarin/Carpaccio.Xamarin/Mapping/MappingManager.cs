
using System.Collections;
using System.Collections.Generic;
using Android.Views;
using Carpaccio.Model;
using Java.Lang;

namespace Carpaccio.Mapping
{
	public class MappingManager {
		private const string TAG = "CarpaccioMappingManager";
		protected Dictionary<string, Object> MappedObjects = new Dictionary<string, Object>();
    protected Dictionary<string, ArrayList> MappedLists = new Dictionary<string,ArrayList>();
    protected Dictionary<string, List<MappingWaiting>> MappingWaitings = new Dictionary<string, List<MappingWaiting>>();

    public interface IMappingManagerCallback 
	{
        void CallActionOnView(CarpaccioAction action, View view);
    }

    protected IMappingManagerCallback MappingManagerCallback;

		/**
     * All mapping call must start with $
     * ex : function($user)   function($user.getText())
     * CAN ONLY MAP 1 OBJECT, function($user1,$user2) will be rejected
     */
    public static bool IsCallMapping(string[] args) {
        return args.Length == 1 && args[0].StartsWith("$");
    }

    //object.image.getUrl()
    public string Evaluate(Object obj, string call) {
        if (!call.Contains(".")) { //"object"
            CarpaccioLogger.d(TAG, "call " + call + " on " + obj.GetType().Name);
            return obj.ToString();
        }
	    string function = call.Substring(call.IndexOf('.') + 1); //image.getUrl(); or //image
	    string callToGetObject;
	    if (function.Contains(".")) {
		    callToGetObject = function.Substring(0, function.IndexOf('.')); //image
	    } else {
		    callToGetObject = function; //image
	    }
	    string realCallToGetObject = GetFunctionName(callToGetObject);
	    Object newObject = CarpaccioHelper.callFunction(obj, realCallToGetObject);
	        
	    if (newObject != null) {
		    CarpaccioLogger.d(TAG, "call " + realCallToGetObject + " return =" + newObject.GetType().Name);

		    if (newObject is Java.Lang.String) {
			    return (string) newObject;
		    }
		    if (newObject is Java.Lang.Number) {
			    return String.ValueOf(newObject);
		    }
		    return Evaluate(newObject, function);
	    }
	    CarpaccioLogger.d(TAG, "call " + realCallToGetObject + " return = NULL");

	    return null;
    }

    /**
     * Add an object to the mapper
     * When the object is added, call all the mappingWaitings (views which need this object)
     *
     * @param name   the mapped object name, ex : for function($user), the name will be "user"
     * @param object the mapped object
     */
    public void MapObject(string name, Object obj) {
        MappedObjects.AddOrUpdate(name, obj);

        CarpaccioLogger.d(TAG, "map object [" + name + "," + obj.GetType().Name + "]");

        //call the waiting objects
        List<MappingWaiting> waitingsForThisName = MappingWaitings.GetOrDefault(name);
        if (waitingsForThisName != null) {
            foreach (MappingWaiting mappingWaiting in waitingsForThisName) {

                CarpaccioLogger.d(TAG, "call waiting mapped " + mappingWaiting.CarpaccioAction.CompleteCall);

                string value = Evaluate(obj, mappingWaiting.Call);

                CarpaccioLogger.d(TAG, "call waiting value =  " + value);

                if (value != null && MappingManagerCallback != null) {
                    mappingWaiting.CarpaccioAction.Values = new[]{value}; //TODO

                    MappingManagerCallback.CallActionOnView(mappingWaiting.CarpaccioAction, mappingWaiting.View);
                }
            }

            //remove all waitings for this name
            waitingsForThisName.Clear();
            MappingWaitings.Remove(name);
        }
    }

    public void MapList(string name, ArrayList list) {
        CarpaccioLogger.d(TAG, "map list " + name + " size=" + list.Count);
        MappedLists.AddOrUpdate(name, list);
    }

	public void AppendList(string mappedName, ArrayList list)
	{
		ArrayList savedList = MappedLists.GetOrDefault(mappedName);
        if (savedList == null) {
            CarpaccioLogger.e(TAG, "No list found for [" + mappedName + "]");
        } else {
            savedList.AddRange(list);
        }
    }


	public ArrayList GetMappedList(string name)
	{
        return MappedLists.GetOrDefault(name);
    }

    /**
     * From getName() return getName()
     * From name return getName()
     */
    protected static string GetFunctionName(string call) {
        if (call.Contains("(") && call.Contains(")"))
            return call.Replace("()", "");
	    
		string firstLetter = call.Substring(0, 1).ToUpperInvariant();
	    string lastLetters = call.Substring(1, call.Length - 1);

	    return "get" + firstLetter + lastLetters;
    }

    /**
     * Called when a view loaded and call a mapping function
     *
     * @param view         the calling view
     * @param mappedObject If available, the object to map with the view. Else add the view to mappingWaitings
     */
    public void CallMappingOnView(CarpaccioAction action, View view, Object mappedObject) {

        if (action.IsCallMapping) {

            CarpaccioLogger.d(TAG, "callMappingOnView mapping=" + mappedObject + " action=" + action.CompleteCall + " view=" + view.GetType().Name);

            string arg = action.Args[0]; //only map the first argument

            string objectName;

            string call;
            if (arg.Contains(".")) { //"$user.getName()"
                call = arg.Substring(1, arg.Length - 1); // "user.getName()"
                objectName = call.Substring(0, arg.IndexOf('.') - 1); // "user"
            } else {
                objectName = arg.Substring(1, arg.Length - 1); // "user"
                call = objectName; // "user"
            }

            //if you already have the object
            if (mappedObject != null) {
                string value = Evaluate(mappedObject, call);

                CarpaccioLogger.d(TAG, "callMappingOnView evaluate(" + call + ")" + " on " + mappedObject.GetType().Name + " returned " + value);

                action.Values = new[]{value}; //TODO

                MappingManagerCallback.CallActionOnView(action, view);
            } else {
                //add to waiting
                List<MappingWaiting> waitings = MappingWaitings.GetOrDefault(objectName) ?? new List<MappingWaiting>(); //["user"] = List<MappingWaiting>
	            waitings.Add(new MappingWaiting(view, action, call, objectName));

                CarpaccioLogger.d(TAG, "added to waiting " + call + " for " + view.GetType().Name);

                MappingWaitings.AddOrUpdate(objectName, waitings);
            }
        }
    }

    public IMappingManagerCallback GetMappingManagerCallback() {
        return MappingManagerCallback;
    }

    public void SetMappingManagerCallback(IMappingManagerCallback mappingManagerCallback) {
        MappingManagerCallback = mappingManagerCallback;
    }

    public object GetMappedListsObject(string name, int position) {
        return MappedLists[name][position];
    }

    public object GetMappedObject(string name) {
        return MappedObjects.GetOrDefault(name);
    }
}
}