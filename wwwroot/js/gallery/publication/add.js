let currentAbortController = null;
let preuploadResult = null;
async function PreloadFile(){
    if (currentAbortController) {
        currentAbortController.abort();
    }
    currentAbortController = new AbortController();

    const mediaInput = document.getElementById("Media");
    if (!mediaInput || !mediaInput.files.length) {
        return;
    }
    const file = mediaInput.files[0];
    const formData = new FormData();
    formData.append("file", file)

    try{
            const response = await fetch("/Static/preload", {
            method: "POST",
            body: formData,
            signal: currentAbortController.signal
        }).then(result => {
            console.log("File upload and analysis result:", result);
            preuploadResult = result.json();
            //alert("File successfully analyzed!");
        });
        //TODO: Show warning with info about limits
        //if (!response.ok) {
        //    throw new Error(`Error: ${response.statusText}`);
        //}
    }
    catch(e){
        throw new Error(`Error: ${e}`);
    }
}

async function upload(){
    if(preuploadResult == null)
        throw new Error('No file was preloaded yet');
    
    const IsPublic = !($("#IsNSFW")[0].checked);
    const Author = $("#MediaAuthor")[0].value;
    const IsExclusiveToAuthor = $("#IsExclusiveToAuthor")[0].checked;
    const data = JSON.stringify(
        {
            "IsPublic": IsPublic,
            "Author": Author,
            "IsExclusiveToAuthor": IsExclusiveToAuthor
        }
    );

    try{
        const response = await fetch("/Static/commit", {
            method: "POST",
            body: data,
            headers:{
                "Content-Type" : "application/json"
            }
        })
        return response.text;
    }
    catch(e){return null}
}

window.addEventListener("beforeunload", () => {
    if (currentAbortController) {
        currentAbortController.abort();
    }
});

$("#Media").change(PreloadFile);

document.getElementById("publicationForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Prevent the default form submission

    const form = document.getElementById("publicationForm");
    const submitButton = document.getElementById("submitButton");
    const loadingSpinner = document.getElementById("loadingSpinner");

    // Disable the form and show the loading spinner
    submitButton.disabled = true;
    //loadingSpinner.style.display = "block";
    const uploadGuid = await upload();
    if(uploadGuid == null){
        throw new Error("Failed to upload media");
    }
    // Create FormData object to handle file upload
    const formData = new FormData(form);
    formData.set("Media", uploadGuid);
    try {
        // Send form data asynchronously with Fetch API
        const response = await fetch(form.action, {
            method: "POST",
            body: formData
        });

        // Hide the loading spinner
        //loadingSpinner.style.display = "none";

        if (response.ok) {
            //TODO: redirect to the publication page and make post request add popup messages to the bag    
        } else {
            // If response has errors, re-enable form and display an error message
            submitButton.disabled = false;
            alert("Error: Could not create publication. Please try again.");
        }
    } catch (error) {
        // Handle network or other errors
        console.error("Error submitting form:", error);
        alert("An error occurred. Please try again.");
        submitButton.disabled = false;
        //loadingSpinner.style.display = "none";
    }
});