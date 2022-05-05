mergeInto(LibraryManager.library, {
  TriggerReactStringEvent: function (eventName, dataString) {
    window.dispatchReactUnityEvent(
      Pointer_stringify(eventName),
      Pointer_stringify(dataString)
    );
  },
  TriggerReactIntEvent: function (eventName, dataInt) {
    window.dispatchReactUnityEvent(
      Pointer_stringify(eventName),
      dataInt
    );
  }
});