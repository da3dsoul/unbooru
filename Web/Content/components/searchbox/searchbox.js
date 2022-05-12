import axios from "axios";
import React, {useState, useCallback, Fragment} from 'react';
import {Input, Ul, Li, SuggestContainer, Span} from './style';
import debounce from 'lodash.debounce';
import regeneratorRuntime from "regenerator-runtime";

const searchTags = ['tag:', 'tagID:', 'pixivID:', 'artist:', 'artistID:', 'aspect:', 'filesize:', 'sfw'];
const tagUrl = axios.create({baseURL: '/api/Tag/ByName/'});
const artistUrl = axios.create({baseURL: '/api/Artist/ByName/'});

const getApiSuggestions = async (word) => {
    const groups = word.split(',');
    const autofillType = getAutofillType(groups);
    if (autofillType.type === 'term') {
        // no search terms specified
        // the autofillType should be 'term' here
        const last = groups.slice(-1)[0].trim()
        let terms = []
        for(let i = 0; i < searchTags.length; i++) {
            const current = searchTags[i];
            if (current.startsWith(last)) {
                terms.push({ type: 'term', name: current });
            }
        }

        if (terms.length > 0) return terms;
    } else {
        if (autofillType.type === 'tag:' && autofillType.query !== '') {
            return await tagUrl
                .get(`/${encodeURIComponent(autofillType.query)}`)
                .then((response) => {
                    return response.data.map(a => ({ type: autofillType.type.replace(':', ''), name: a.name }));
                })
                .catch((error) => {
                    return error;
                });
        } else if (autofillType.type === 'artist:' && autofillType.query !== '') {
            return await artistUrl
                .get(`/${encodeURIComponent(autofillType.query)}`)
                .then((response) => {
                    return response.data.map(a => ({ type: autofillType.type.replace(':', ''), name: a.name }));
                })
                .catch((error) => {
                    return error;
                });
        }
    }
};

function getAutofillType(groups) {
    const group = groups[Math.max(groups.length - 1, 0)].trim();
    for (let j = 0; j < searchTags.length; j++) {
        const term = searchTags[j];
        const replaced = group.replace(term, '').trim();
        if (group.length !== replaced.length) {
            return { type: term, query: replaced};
        }
    }

    return { type: 'term', query: group };
}

function findOverlap(a, b) {
    if (b.length === 0) {
        return "";
    }

    if (a.endsWith(b)) {
        return b;
    }

    if (a.indexOf(b) >= 0) {
        return b;
    }

    return findOverlap(a, b.substring(0, b.length - 1));
}

export default function SearchInput({ placeholder, }) {
    const [inputValue, setInputValue] = useState('');
    const [options, setOptions] = useState([]);
    const [loading, setLoading] = useState(false);

    const fillBox = (data) => {
        let text = inputValue;
        const overlap = findOverlap(text, data.name);
        if (text.endsWith(',') || text.endsWith(', ')) return;
        if (!text.endsWith(overlap)) return;
        text += data.name.replace(overlap, '') + ', ';
        setInputValue(text)
    };

    const getTagApiUrl = (data) => {
        const name = encodeURIComponent(data.name)
        window.open(`/Search?tag=${name}`, '_blank');
    };
    
    const search = (text) => {
        let url = '/Search?';
        const queries = text.split(',');
        for (let i = 0; i < queries.length; i++) {
            const query = queries[i].trim();
            if (query === '') continue;
            if (!url.endsWith('&') && !url.endsWith('?')) url += '&';
            if (query === 'sfw') {
                url += 'sfw';
            } else {
                const parts = query.split(':');
                if (parts.length !== 2) continue;
                const term = parts[0].trim();
                const value = parts[1].trim();
                url += term + '=' + encodeURIComponent(value);
            }
        }
        window.open(url, '_blank');
    }

    const getSuggestions = async (word) => {
        if (word) {
            setLoading(true);
            let response = await getApiSuggestions(word);
            setOptions(response);
            setLoading(false);
        } else {
            setOptions([]);
        }
    };

    const debouncedSave = useCallback(debounce((newValue) => getSuggestions(newValue), 1000), []);

    const updateValue = (newValue) => {
        setInputValue(newValue);
        debouncedSave(newValue);
    };

    return (
        <Fragment>
            <div>
                <Input value={inputValue} tabIndex="0" onChange={(input) => updateValue(input.target.value)} onKeyDown={(evt) => {
                    if(evt.keyCode === 13) {
                        search(inputValue);
                    }
                }} placeholder={placeholder} />
                <SuggestContainer>
                    {loading || options?.length > 0 && <Ul>
                        {loading && <Li key='loading'>Loading...</Li>}
                        {options?.length > 0 &&
                            !loading &&
                            options?.map((value, index) => (
                                <Li key={`${value.name}-${index}`} onClick={() => fillBox(value)}>
                                    <Span>{value.name}</Span>
                                </Li>
                            ))}
                    </Ul>}
                </SuggestContainer>
            </div>
            <div className="Nav__item">
                <div className="Nav__link" onClick={() => search(inputValue)}>Search</div>
            </div>
        </Fragment>
    );
}