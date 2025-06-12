const lostItemInput = document.getElementById("lostSearch");
const searchTypeSelect = document.getElementById("SearchTypID");
const emailInput = document.getElementById("email");
const typeSearchDiv = document.getElementById("searchType_div");
const form = document.getElementById("Check-lostItems-form");
let foundItems = document.getElementById("foundItemsId");
let formSection = document.getElementById("formSection");

function showSpinner() {
  document.getElementById("spinner").style.display = "block";
}

function hideSpinner() {
  document.getElementById("spinner").style.display = "none";
}

document.addEventListener("DOMContentLoaded", function () {
  lostItemInput.addEventListener("change", function () {
    const value = lostItemInput.value.trim();
    if (value !== "") {
      typeSearchDiv.classList.remove("d-none");
      typeSearchDiv.classList.add("d-flex");
      clearErrors();
    } else {
      typeSearchDiv.classList.add("d-none");
      typeSearchDiv.classList.remove("d-flex");
    }
  });

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    clearErrors();

    const lostItem = lostItemInput.value.trim();
    const searchTypeSelectValue = searchTypeSelect.value;
    const email = emailInput.value.trim();

    let hasError = false;

    if (!lostItem) {
      showError("err_lostSearch_id", "Lost item is required");
      hasError = true;
    }

    if (!searchTypeSelectValue) {
      showError("err_SearchTyp_id", "Search Type is required");
      hasError = true;
    }

    if (!email || !/\S+@\S+\.\S+/.test(email)) {
      showError("err_email", "Valid email is required");
      hasError = true;
    }

    if (hasError) return;

    showSpinner();

    try {
      if (searchTypeSelectValue == "Image") {
        await getFoundDataFromAPI(lostItem, email, searchTypeSelectValue);
      } else if (searchTypeSelectValue == "Text") {
        await getFoundDataFromAPI(lostItem, email, searchTypeSelectValue);
      } else if (searchTypeSelectValue == "Both") {
        await getFoundDataFromAPI(lostItem, email, searchTypeSelectValue);
      }
    } catch (error) {
      console.error("Error while fetching data:", error);
    } finally {
      hideSpinner();
    }

  });

  function clearErrors() {
    showError("err_lostSearch_id", "");
    showError("err_SearchTyp_id", "");
    showError("err_email", "");
  }

  function showError(elementId, message) {
    document.getElementById(elementId).textContent = message;
  }
});

function showSpinner() {
  document.getElementById("loadingSpinner").classList.remove("d-none");
}

function hideSpinner() {
  document.getElementById("loadingSpinner").classList.add("d-none");
}

async function getFoundDataFromAPI(item, email, matchType) {
  if (item === "card") {
    const token = localStorage.getItem("token");
    showSpinner();
    console.log("Sending request to match card"),
      $.ajax({
        url: `http://localhost:5194/api/Checking_For_Items/CardMatch?email=${encodeURIComponent(
          email
        )}&matchType=${encodeURIComponent(matchType)}`,
        type: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        success: function (data) {
          console.log("Success response:", data);

          hideSpinner();
          showFoundItems(data, item, matchType);

          if (data.length === 0) {
            // alert("⚠ No matching cards were found.");
            ShowBootstrapToast("No matching cards were found", "danger");
          } else {
            // alert("Matching complete! Check console for results.");
            ShowBootstrapToast(
              " Matching complete, Wati for showing your matching  ",
              "success",
              true
            );
          }
        },
        error: function (xhr, status, error) {
          console.log("error response");

          hideSpinner();

          let message = "Unexpected error occurred.";

          if (xhr.status === 400) {
            message = xhr.responseText;
          } else if (xhr.status === 500) {
            message = "Server Error: " + xhr.responseText;
          } else if (xhr.status === 0) {
            message = "Request failed. Are you offline or is the server down?";
          }

          console.error("Error Details:", {
            status: xhr.status,
            response: xhr.responseText,
            error: error,
          });

          ShowBootstrapToast(message, "danger");
        },
      });
  } else if (item === "Phone") {
    const token = localStorage.getItem("token");
    showSpinner();
    console.log("Sending request to match phone"),
      $.ajax({
        url: `http://localhost:5194/api/Checking_For_Items/matchPhone?email=${encodeURIComponent(
          email
        )}&matchType=${encodeURIComponent(matchType)}`,
        type: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        success: function (data) {
          hideSpinner();
          displayResults(data);
          if (data.length === 0) {
            alert("No matching cards were found.");
          } else {
            alert("Matching complete! Check console for results.");
            ShowBootstrapToast(
              " Matching complete! Check console for results ",
              "success",
              true
            );
            showFoundItems(data, item, matchType);
          }
        },
        error: function (xhr, status, error) {
          hideSpinner();
          let message = "Unexpected error occurred.";

          if (xhr.status === 400) {
            message = xhr.responseText;
          } else if (xhr.status === 500) {
            message = "Server Error: " + xhr.responseText;
          } else if (xhr.status === 0) {
            message = "Request failed. Are you offline or is the server down?";
          }

          console.error("Error Details:", {
            status: xhr.status,
            response: xhr.responseText,
            error: error,
          });

          ShowBootstrapToast(message, "danger");
        },
      });
  }
}

function showFormAgain() {
  foundItems.classList.remove("d-flex");
  foundItems.classList.add("d-none");
  formSection.classList.remove("d-none");
  formSection.classList.add("d-flex");

  // مسح الفورم
  document.getElementById("lostSearch").value = "";
  document.getElementById("SearchTypID").value = "";
  document.getElementById("email").value = "";
  document.getElementById("searchType_div").classList.add("d-none");
  document.querySelector("#link").classList.add("d-none");
  document.querySelector("#link").classList.remove("d-block");
}

document.querySelector("#link a").addEventListener("click", () => {
  showFormAgain();
});

function showFoundItems(items, lostItem, searchTypeItem) {
  let arrItms = items;
  foundItems.innerHTML = "";
  formSection.classList.remove("d-flex");
  formSection.classList.add("d-none");
  foundItems.classList.remove("d-none");
  foundItems.classList.add("d-flex");
  document.querySelector("#link").classList.remove("d-none");
  document.querySelector("#link").classList.add("d-block");

  if (!arrItms || arrItms.length === 0) {
    return;
  }
  if (lostItem === "card") {
    ShowBootstrapToast("Cards Found", "success", true);

    arrItms.forEach((e) => {
      let createDivParent = document.createElement("div");
      createDivParent.classList.add("card", "mb-3", "p-3");
      createDivParent.style.width = "20rem";
      foundItems.appendChild(createDivParent);

      let createMainHeder = document.createElement("h4");
      createMainHeder.classList.add("text-center");
      createMainHeder.textContent = `${lostItem} has been found`;
      createDivParent.appendChild(createMainHeder);

      if (searchTypeItem !== "Text") {
        let createMainDiveImgs = document.createElement("div");
        createMainDiveImgs.classList.add(
          "d-flex",
          "justify-content-between",
          "mb-3",
          "text-center"
        );
        createDivParent.appendChild(createMainDiveImgs);

        let createLostImg = document.createElement("div");
        createLostImg.classList.add("me-3");
        createMainDiveImgs.appendChild(createLostImg);

        let createLostHeader = document.createElement("h5");
        createLostHeader.classList.add("card-title");
        createLostHeader.textContent = "Lost Image";

        let createLostImageSRC = document.createElement("img");
        createLostImageSRC.classList.add(
          "card-img-top",
          "border",
          "border-3",
          "border-danger"
        );
        createLostImageSRC.src = e.face_images.lost_face;
        createLostImageSRC.alt = "No Image Found";
        createLostImageSRC.style.maxWidth = "150px";
        createLostImageSRC.style.height = "auto";

        createLostImg.appendChild(createLostHeader);
        createLostImg.appendChild(createLostImageSRC);

        let createFoundImg = document.createElement("div");
        createMainDiveImgs.appendChild(createFoundImg);

        let createFoundHeader = document.createElement("h5");
        createFoundHeader.classList.add("card-title");
        createFoundHeader.textContent = "Found Image";

        let createFoundImageSRC = document.createElement("img");
        createFoundImageSRC.classList.add(
          "card-img-top",
          "border",
          "border-3",
          "border-success"
        );
        createFoundImageSRC.src = e.face_images.found_face;
        createFoundImageSRC.alt = "No Found Image";
        createFoundImageSRC.style.maxWidth = "150px";
        createFoundImageSRC.style.height = "auto";

        createFoundImg.appendChild(createFoundHeader);
        createFoundImg.appendChild(createFoundImageSRC);
      }

      let createDivData = document.createElement("div");
      createDivData.classList.add("card-body");

      let TextSimilarity = document.createElement("h5");
      TextSimilarity.classList.add("card-title", "d-block", "mb-2");
      TextSimilarity.innerHTML = `Text Similarity: <span class="text-success">${e.text_similarity}</span>`;

      let FaceVerified = document.createElement("h5");
      FaceVerified.classList.add("card-title", "d-block", "mb-2");
      FaceVerified.innerHTML = `Face Verified: <span class="text-success">${
        e.face_verified ? "True" : "False"
      }</span>`;

      let FaceDistance = document.createElement("h5");
      FaceDistance.classList.add("card-title", "d-block", "mb-2");
      FaceDistance.innerHTML = `FaceDistance: <span class="text-success">${e.face_distance}</span>`;

      let MatchResult = document.createElement("h5");
      MatchResult.classList.add("card-title", "d-block", "mb-2");
      MatchResult.innerHTML = `Match Result: <span class="text-success">${
        e.match_result ? "Match" : "Not Match"
      }</span>`;

      let ContactInfo = document.createElement("h5");
      ContactInfo.classList.add("card-title", "d-block", "mb-2");
      ContactInfo.innerHTML = `Contact Info: <span class="text-success">${e.contact_info.found}</span>`;

      createDivParent.appendChild(createDivData);
      createDivData.appendChild(TextSimilarity);
      createDivData.appendChild(FaceVerified);
      createDivData.appendChild(FaceDistance);
      createDivData.appendChild(MatchResult);
      createDivData.appendChild(ContactInfo);
    });
    ShowBootstrapToast("Phone Found", "success", true);

    arrItms.forEach((e) => {
      let createDivParent = document.createElement("div");
      createDivParent.classList.add("card", "mb-3", "p-3");
      createDivParent.style.width = "20rem";
      foundItems.appendChild(createDivParent);

      let createMainHeder = document.createElement("h4");
      createMainHeder.classList.add("text-center");
      createMainHeder.textContent = `${lostItem} has been found`;
      createDivParent.appendChild(createMainHeder);

      if (searchTypeItem !== "Text") {
        let createMainDiveImgs = document.createElement("div");
        createMainDiveImgs.classList.add(
          "d-flex",
          "justify-content-between",
          "mb-3",
          "text-center"
        );
        createDivParent.appendChild(createMainDiveImgs);

        let createLostImg = document.createElement("div");
        createLostImg.classList.add("me-3");
        createMainDiveImgs.appendChild(createLostImg);

        let createLostHeader = document.createElement("h5");
        createLostHeader.classList.add("card-title");
        createLostHeader.textContent = "Match Image Found";

        let createLostImageSRC = document.createElement("img");
        createLostImageSRC.classList.add(
          "card-img-top",
          "border",
          "border-3",
          "border-success"
        );
        createLostImageSRC.src = e.face_images.lost_face;
        createLostImageSRC.alt = "No Image Found";
        createLostImageSRC.style.maxWidth = "100%";
        createLostImageSRC.style.height = "auto";

        createLostImg.appendChild(createLostHeader);
        createLostImg.appendChild(createLostImageSRC);
      }

      let createDivData = document.createElement("div");
      createDivData.classList.add("card-body");

      let TextSimilarity = document.createElement("h5");
      TextSimilarity.classList.add("card-title", "d-block", "mb-2");
      TextSimilarity.innerHTML = `Text Similarity: <span class="text-success">${e.text_similarity}</span>`;

      let FaceVerified = document.createElement("h5");
      FaceVerified.classList.add("card-title", "d-block", "mb-2");
      FaceVerified.innerHTML = `Face Verified: <span class="text-success">${
        e.face_verified ? "True" : "False"
      }</span>`;

      let FaceDistance = document.createElement("h5");
      FaceDistance.classList.add("card-title", "d-block", "mb-2");
      FaceDistance.innerHTML = `FaceDistance: <span class="text-success">${e.text_best_match}</span>`;

      let MatchResult = document.createElement("h5");
      MatchResult.classList.add("card-title", "d-block", "mb-2");
      MatchResult.innerHTML = `Match Result: <span class="text-success">${
        e.match_result ? "Match" : "Not Match"
      }</span>`;

      let ContactInfo = document.createElement("h5");
      ContactInfo.classList.add("card-title", "d-block", "mb-2");
      ContactInfo.innerHTML = `Contact Info: <span class="text-success">${e.contact_info.found}</span>`;

      createDivParent.appendChild(createDivData);
      createDivData.appendChild(TextSimilarity);
      createDivData.appendChild(FaceVerified);
      createDivData.appendChild(FaceDistance);
      createDivData.appendChild(MatchResult);
      createDivData.appendChild(ContactInfo);
    });

    // -----------------------  TostBox (Popup Message)    ----------------------------------
    window.ShowBootstrapToast = function (
      message,
      type = "Info",
      withButtons = false
    ) {
      const toastId = "custom-toast-" + Date.now();
      const toastHTML = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type.toLowerCase()} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body w-100">
                        ${message}
                        ${
                          withButtons
                            ? `
                        <div class="mt-2 pt-2 border-top d-flex justify-content-end gap-2">
                            <button type="button" class="btn btn-light btn-sm" id="btn-add-new">Check another Item</button>
                            <button type="button" class="btn btn-outline-light btn-sm" id="btn-go-home">Go Home</button>
                        </div>`
                            : ""
                        }
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

      let toastContainer = document.getElementById("toast-container");
      if (!toastContainer) {
        toastContainer = document.createElement("div");
        toastContainer.id = "toast-container";
        toastContainer.className =
          "toast-container position-fixed bottom-0 end-0 p-3";
        document.body.appendChild(toastContainer);
      }

      toastContainer.innerHTML += toastHTML;

      const toastElement = document.getElementById(toastId);
      const toast = new bootstrap.Toast(toastElement, { delay: 7000 });
      toast.show();

      if (withButtons) {
        toastElement
          .querySelector("#btn-add-new")
          .addEventListener("click", function () {
            showFormAgain();
            $("#Check-lostItems-form")[0].reset();
            $("#preview").attr(
              "src",
              "../../images/id-card-illustration_23-2147829294.avif"
            );
            $(".error-msg").text("");
            toast.hide();
          });

        toastElement
          .querySelector("#btn-go-home")
          .addEventListener("click", function () {
            window.location.replace("../../../index.html");
          });
      }

      toastElement.addEventListener("hidden.bs.toast", function () {
        toastElement.remove();
      });
    };

    //--------------------------------------------------------------------------------
    function showSpinner() {
      document.getElementById("loadingSpinner").classList.remove("d-none");
    }

    function hideSpinner() {
      document.getElementById("loadingSpinner").classList.add("d-none");
    }
  }
}

// const sampleData = [
//   {
//     match_type: "Both",
//     matched: 1,
//     final_score: 0.95,
//     text_similarity: 0.92,
//     text_best_match: {
//       governorate: "Cairo",
//       city: "Nasr City",
//       street: "Mostafa El-Nahas Street",
//       contact: "maged@gmail.com",
//       brand: "iPhone",
//       color: "Black",
//       image_url: "https://example.com/image1.jpg",
//     },
//     image_similarity: 0.98,
//     matched_images: [
//       {
//         image_url: "https://example.com/match1.jpg",
//         image_similarity: 0.98,
//         associated_data: null,
//       }
//     ],
//   },
// ];

function hideDivs() {
  document.getElementById("results").innerHTML = "";
  formSection.classList.remove("d-flex");
  formSection.classList.add("d-none");
  document.querySelector("#link").classList.remove("d-none");
  document.querySelector("#link").classList.add("d-block");
}

// Helper function to create element with attributes and content
function createElement(tag, attributes = {}, textContent = "", children = []) {
  const element = document.createElement(tag);

  // Set attributes
  Object.keys(attributes).forEach((attr) => {
    if (attr === "className") {
      element.className = attributes[attr];
    } else {
      element.setAttribute(attr, attributes[attr]);
    }
  });

  // Set text content
  if (textContent) {
    element.textContent = textContent;
  }

  // Append children
  children.forEach((child) => {
    if (child) {
      element.appendChild(child);
    }
  });

  return element;
}

// Create detail item element
function createDetailItem(label, value) {
  const labelSpan = createElement("span", { className: "detail-label" }, label);
  const valueSpan = createElement("span", { className: "detail-value" }, value);
  return createElement("div", { className: "detail-item" }, "", [
    labelSpan,
    valueSpan,
  ]);
}

// Create score card element
function createScoreCard(value, label) {
  const scoreValue = createElement(
    "div",
    { className: "score-value" },
    `${(value * 100).toFixed(1)}%`
  );
  const scoreLabel = createElement("div", { className: "score-label" }, label);
  return createElement("div", { className: "score-card" }, "", [
    scoreValue,
    scoreLabel,
  ]);
}

// Create phone details section
function createPhoneDetailsSection(phoneData) {
  if (!phoneData) return null;

const title = createElement("h3", {}, "Phone Details");

const phoneImage = document.createElement("img");
phoneImage.src = phoneData.image_url;
phoneImage.alt = "Phone Image";
phoneImage.style.maxWidth = "200px"; // تعديل حسب الحجم المطلوب

const detailsGrid = createElement("div", { className: "details-grid" }, "", [
  createDetailItem("Governorate:", phoneData.governorate),
  createDetailItem("City:", phoneData.city),
  createDetailItem("Street:", phoneData.street),
  createDetailItem("Brand:", phoneData.brand),
  createDetailItem("Color:", phoneData.color),
  createDetailItem("Contact:", phoneData.contact),
]);

// سطر منفصل للصورة
detailsGrid.appendChild(phoneImage);

return createElement("div", {}, "", [title, detailsGrid]);
}

// Create image card element
function createImageCard(imageData) {
  const phoneImage = document.createElement("img");
phoneImage.src = imageData.image_url;
phoneImage.alt = "Phone Image";
phoneImage.style.maxWidth = "200px"; 


  const imagePlaceholder = createElement(
    "div",
    { className: "image-placeholder" }
  );
  imagePlaceholder.appendChild(phoneImage);
  const imageSimilarity = createElement(
    "div",
    { className: "image-similarity" },
    `Similarity: ${(imageData.image_similarity * 100).toFixed(1)}%`
  );

  const children = [imagePlaceholder, imageSimilarity];

  if (imageData.associated_data !== null) {
    const detailsGrid = createElement(
      "div",
      { className: "details-grid" },
      "",
      [

        
        createElement("div", { className: "detail-item" }, "", [
        createElement(
          "div",
            { className: "details-grid" },
             "Phone Details", 
        ),
        
         createElement(
            "span",
            { className: "detail-label" },
            imageData.associated_data.brand
          ),
          createElement(
            "span",
            { className: "detail-value" },
            imageData.associated_data.color
          ),

        ]),
        createElement("div", { className: "detail-item" }, "", [
           createElement(
          "div",
            { className: "details-grid" },
              "Found Phone Location"  , 
        ),
        
            createElement(
            "span",
            { className: "detail-label" },
            imageData.associated_data.governorate
          ), 
          createElement(
            "span",
            { className: "detail-label" },
            imageData.associated_data.city
          ),
            createElement(
            "span",
            { className: "detail-label" },
            imageData.associated_data.street
          ),
      
        ]),
        createElement("div", { className: "detail-item" }, "", [
          createElement(
          "div",
            { className: "details-grid" },
          "Contact With Founder"    , 
        ),
        
      
            createElement(
            "span",
            { className: "detail-label" },
            imageData.associated_data.contact
          ), 
        

        ]),
      ]
    );
    children.push(detailsGrid);
  }

  return createElement("div", { className: "image-card" }, "", children);
}

// Create images section
function createImagesSection(matchedImages) {
  if (!matchedImages || matchedImages.length === 0) return null;

  const title = createElement(
    "h4",
    {},
    `Matched Images (${matchedImages.length})`
  );

  const imageCards = matchedImages.map((img) => createImageCard(img));
  const imagesGrid = createElement(
    "div",
    { className: "images-grid" },
    "",
    imageCards
  );

  return createElement("div", { className: "images-section" }, "", [
    title,
    imagesGrid,
  ]);
}

// Create match card element
function createMatchCard(result) {
  // Create card header
  const matchType = createElement(
    "div",
    { className: "match-type" },
    result.match_type
  );

  const statusIndicator = createElement("div", {
    className: `status-indicator ${
      result.matched ? "status-matched" : "status-not-matched"
    }`,
  });
  const statusText = createElement(
    "span",
    {},
    result.matched ? "Match Found" : "No Match"
  );
  const matchStatus = createElement("div", { className: "match-status" }, "", [
    statusIndicator,
    statusText,
  ]);

  const cardHeader = createElement("div", { className: "card-header" }, "", [
    matchType,
    matchStatus,
  ]);

  // Create scores section
  const scoresSection = createElement(
    "div",
    { className: "scores-section" },
    "",
    [
      createScoreCard(result.final_score, "Final Score"),
      createScoreCard(result.text_similarity, "Text Similarity"),
      createScoreCard(result.image_similarity, "Image Similarity"),
    ]
  );

  // Create card content
  const cardChildren = [cardHeader, scoresSection];

  // Add phone details if available
  const phoneDetails = createPhoneDetailsSection(result.text_best_match);
  if (phoneDetails) {
    cardChildren.push(phoneDetails);
  }

  // Add images section if available
  const imagesSection = createImagesSection(result.matched_images);
  if (imagesSection) {
    cardChildren.push(imagesSection);
  }

  return createElement("div", { className: "match-card" }, "", cardChildren);
}

// Create no results message
function createNoResultsMessage() {
  const title = createElement("h3", {}, "No Results Found");
  const message = createElement("p", {}, "No matches were found");
  return createElement("div", { className: "no-results" }, "", [
    title,
    message,
  ]);
}

// Create error message
function createErrorMessage() {
  const title = createElement("h3", {}, "Error Loading Data");
  const message = createElement("p", {}, "Unable to connect to server");
  return createElement("div", { className: "no-results" }, "", [
    title,
    message,
  ]);
}

// Main display function using DOM methods
function displayResults(data) {
  hideDivs();
  const resultsContainer = document.getElementById("results");

  // Clear existing content
  while (resultsContainer.firstChild) {
    resultsContainer.removeChild(resultsContainer.firstChild);
  }

  if (!data || data.length === 0) {
    resultsContainer.appendChild(createNoResultsMessage());
    return;
  }

  // Create and append match cards
  data.forEach((result) => {
    const matchCard = createMatchCard(result);
    resultsContainer.appendChild(matchCard);
  });
}

function loadSampleData() {
  displayResults(sampleData);
}

// Function to load data from API (you can modify this)
// async function loadDataFromAPI() {
//   try {
//     // Replace with your actual API endpoint
//     const response = await fetch("/api/mobile-match");
//     const data = await response.json();
//     displayResults(data);
//   } catch (error) {
//     console.error("Error loading data:", error);
//     const resultsContainer = document.getElementById("results");

//     // Clear existing content
//     while (resultsContainer.firstChild) {
//       resultsContainer.removeChild(resultsContainer.firstChild);
//     }

//     resultsContainer.appendChild(createErrorMessage());
//   }
// }
