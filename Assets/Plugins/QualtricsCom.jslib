mergeInto(LibraryManager.library, {

  SendResults: function (results) {
    window.parent.postMessage(Pointer_stringify(results),'*');
	//console.log("Unity sending up: " + Pointer_stringify(results));
  },
});