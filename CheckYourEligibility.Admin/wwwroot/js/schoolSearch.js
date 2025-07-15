function searchSchool(query, index) {
    var id = `ChildList[${index}].school.Name`
    if (query.length >= 3 && query !== null) {
        // Updated to use new SearchSchools endpoint
        fetch('/Check/SearchSchools?query=' + encodeURIComponent(query))
            .then(response => {
                if (!response.ok) {
                    throw new Error('Search failed');
                }
                return response.json();
            })
            .then(data => {
                document.getElementById(`schoolList${index}`).innerHTML = '';
                let counter = 0;
                // loop response and add li elements with an onclick event listener to select the school
                data.forEach(function (value) {
                    var li = document.createElement('li');
                    li.setAttribute('id', value.id);
                    li.setAttribute('value', `${value.name}`);
                    // Check if the counter is even, if not add the 'autocomplete__option--odd' class
                    li.setAttribute('class', counter % 2 === 0 ? 'autocomplete__option' : 'autocomplete__option autocomplete__option--odd')
                    li.innerHTML = `${value.name}, ${value.id}, ${value.postcode}, ${value.la}`;
                    li.addEventListener('click', function () {
                        selectSchool(value.name, value.id, value.la, value.postcode, index);
                    });
                    document.getElementById(`schoolList${index}`).appendChild(li);
                    counter++;
                });
            })
            .catch(error => {
                console.error('Error searching schools:', error);
                document.getElementById(`schoolList${index}`).innerHTML = '<li class="autocomplete__option">Error searching schools</li>';
            });
    } else {
        document.getElementById(`schoolList${index}`).innerHTML = '';
    }
}

function selectSchool(school, urn, la, postcode, index) {
    // element_ids
    var schoolName = `ChildList[${index}].School.Name`;
    var schoolURN = `ChildList[${index}].School.URN`;
    var schoolPostcode = `ChildList[${index}].School.Postcode`;
    var schoolLA = `ChildList[${index}].School.LA`;
    var schoolSearch = `ChildList[${index}].School`
    // set values
    document.getElementById(schoolName).value = school;
    document.getElementById(schoolURN).value = urn;
    document.getElementById(schoolPostcode).value = postcode;
    document.getElementById(schoolLA).value = la;
    document.getElementById(schoolSearch).value = `${school}, ${urn}, ${postcode}, ${la}`;
    // set in local storage
    localStorage.setItem(`schoolName${index}`, school);
    localStorage.setItem(`schoolURN${index}`, urn);
    localStorage.setItem(`schoolPostcode${index}`, postcode);
    localStorage.setItem(`schoolLA${index}`, la);
    // clear options
    document.getElementById(`schoolList${index}`).innerHTML = '';
}

// Set up event listeners for all school search inputs
let schoolSearch = document.getElementsByClassName("school-search");
for (let i = 0; i < schoolSearch.length; i++) {
    schoolSearch[i].oninput = function () {
        searchSchool(this.value, i)
    }
}