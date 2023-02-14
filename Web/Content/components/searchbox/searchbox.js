import axios from "axios";
import React, {useState, useCallback, Fragment, useEffect} from 'react';
import {Input, Ul, Li, SuggestContainer, Span} from './style';
import debounce from 'lodash.debounce';
import regeneratorRuntime from "regenerator-runtime";
import queryString from "query-string";

const searchTags = ['tag:', 'tagID:', 'pixivID:', 'artist:', 'artistID:', 'aspect:', 'filesize:', 'sfw', 'monochrome'];
const tagUrl = axios.create({baseURL: '/api/Tag/ByName/'});
const artistUrl = axios.create({baseURL: '/api/Artist/ByName/'});

const getApiSuggestions = async (word) => {
    const groups = word.split(',');
    const autofillType = getAutofillType(groups);
    if (autofillType.type === 'term') {
        // no search terms specified
        // the autofillType should be 'term' here
        let last = groups.slice(-1)[0].trim()
        const excludes = last.startsWith('!');
        if (excludes) last = last.substring(1);
        let terms = []
        for(let i = 0; i < searchTags.length; i++) {
            const current = searchTags[i];
            if (current.startsWith(last.toLowerCase())) {
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
    let group = groups[Math.max(groups.length - 1, 0)].trim();
    let excludes = group.startsWith('!');
    if (excludes) group = group.substring(1);
    for (let j = 0; j < searchTags.length; j++) {
        const term = searchTags[j];
        let replaced = group.toLowerCase().replace(term, '').trim();
        excludes = replaced.startsWith('!');
        if (excludes) replaced = replaced.substring(1);

        if (group.length !== replaced.length) {
            return { type: term, query: replaced};
        }
    }

    return { type: 'term', query: group };
}

export default function SearchInput({ placeholder, location, }) {
    const [inputValue, setInputValue] = useState('');
    const [options, setOptions] = useState([]);
    const [loading, setLoading] = useState(false);
    
    useEffect(() => {
        if (!location.includes('?')) return;
        const query = location.slice(location.indexOf('?') + 1);
        const params = queryString.parse(query);
        let text = '';
        for (const param in params) {
            const value = params[param];
            if (value !== null && value !== undefined && value !== '') {
                // things like tag: 1girl
                if (Array.isArray(value)) {
                    for (const valuePartKey in value) {
                        const valuePart = value[valuePartKey];
                        const newPart = param + ': ' + valuePart;
                        if (text.length > 0)
                            text += ', ' + newPart;
                        else
                            text += newPart;
                    }
                } else {
                    const newPart = param + ': ' + value;
                    if (text.length > 0)
                        text += ', ' + newPart;
                    else
                        text += newPart;
                }
            } else {
                // things like sfw
                if (text.length > 0)
                    text += ', ' + param;
                else 
                    text += param;
            }
        }
        
        setInputValue(text);
    }, [location])

    const fillBox = (data) => {
        let text = inputValue;
        if (text.endsWith(',') || text.endsWith(', ')) return;
        if (text.includes(data.name + ',')) return;
        let endIndex = Math.max(text.lastIndexOf(':') + 1, text.lastIndexOf(',') + 1);
        const endText = text.substring(endIndex, text.length - 1).trim();
        let excludes = false;
        if (endText.startsWith('!')) excludes = true;
        let newText = text.slice(0, endIndex).trim();
        newText += ' ' + (excludes ? '!' : '') + data.name;
        if (data.type !== 'term' || !data.name.includes(':')) newText += ', ';
        setInputValue(newText);
    };
    
    const search = (text) => {
        let url = '/Search?';
        const queries = text.split(',');
        for (let i = 0; i < queries.length; i++) {
            const query = queries[i].trim();
            if (query === '') continue;
            if (!query.includes(':')) {
                if (!url.endsWith('&') && !url.endsWith('?')) url += '&';
                url += 'sfw';
            } else {
                const parts = query.split(':');
                if (parts.length !== 2) continue;
                const term = parts[0].trim();
                const value = parts[1].trim();
                if (!url.endsWith('&') && !url.endsWith('?')) url += '&';
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
