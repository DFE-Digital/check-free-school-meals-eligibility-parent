function searchSchoolList(query) {
    if (query.length >= 3 && query !== null) {
        fetch('/Check/SearchSchools?query=' + encodeURIComponent(query))
            .then(response => {
                if (!response.ok) {
                    throw new Error('Search failed');
                }
                return response.json();
            })
            .then(data => {
                document.getElementById('schoolListResults').innerHTML = '';
                let counter = 0;
                data.forEach(function (value) {
                    var li = document.createElement('li');
                    li.setAttribute('id', value.id);
                    li.setAttribute('value', value.name);
                    li.setAttribute('class', counter % 2 === 0 ? 'autocomplete__option' : 'autocomplete__option autocomplete__option--odd');
                    li.innerHTML = `${value.name}, ${value.id}, ${value.postcode}, ${value.la}`;
                    li.addEventListener('click', function () {
                        selectSchoolFromList(value.name, value.id, value.la, value.postcode, value.inPrivateBeta);
                    });
                    document.getElementById('schoolListResults').appendChild(li);
                    counter++;
                });
            })
            .catch(error => {
                console.error('Error searching schools:', error);
                document.getElementById('schoolListResults').innerHTML = '<li class="autocomplete__option">Error searching schools</li>';
            });
    } else {
        document.getElementById('schoolListResults').innerHTML = '';
    }
}

function selectSchoolFromList(school, urn, la, postcode, inPrivateBeta) {
    document.getElementById('school-list-search').value = `${school}, ${urn}, ${postcode}, ${la}`;
    document.getElementById('SelectedSchoolURN').value = urn;
    document.getElementById('SelectedSchoolName').value = school;
    document.getElementById('SelectedSchoolLA').value = la;
    document.getElementById('SelectedSchoolPostcode').value = postcode;
    document.getElementById('SelectedSchoolInPrivateBeta').value = inPrivateBeta === true ? 'true' : 'false';
    document.getElementById('schoolListResults').innerHTML = '';
}

// Set up event listener for the school list search input
var schoolListSearchInput = document.querySelector('.school-list-search');
if (schoolListSearchInput) {
    schoolListSearchInput.oninput = function () {
        searchSchoolList(this.value);
    }
}
