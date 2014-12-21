<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Musics extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
        $this->load->library('conversion_age_birthday');
    }

    /**
     * 音楽一覧を取得
     * @param type $search
     * @param type $offset
     * @return type
     */
    function get_list($search = '', $offset = '') {

        $this->db->select('songs.id');
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->select('songs.song_time');
        $this->db->select('songs.genre');
        $this->db->select('songs.release_date');
        $this->db->from('songs');
        //検索条件の生成
        $where = $this->create_where($search);
        $like = $this->create_like($search);
        $this->db->where($where);
        $this->db->like($like);
        $this->config->load('user');
        $this->db->limit($this->config->item('disp_num'), $offset);
        $query = $this->db->get();

        return $query->result();
    }

    /**
     * 音楽一覧の件数を取得
     * @return type
     */
    function get_count_row($search) {
        $this->db->select('songs.id');
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->select('songs.song_time');
        $this->db->select('songs.genre');
        $this->db->select('songs.release_date');
        $this->db->from('songs');
        //検索条件の生成
        $where = $this->create_where($search);
        $like = $this->create_like($search);
        $this->db->where($where);
        $this->db->like($like);

        $query = $this->db->get();

        return $query->num_rows();
    }

    function get_music($id = '') {
        $this->db->where('id', $id);
        $this->db->from('songs');
        $query = $this->db->get();

        if ($query->num_rows() !== 1) {
            echo $query->num_rows();
            show_404();
            eixt();
        }

        return $query->row();
    }

    function create_where($search = '') {
        $where = array();
        if (!empty($search['release_date_over'])) {
            $where += array('release_date >=' => $search['release_date_over']);
        }
        if (!empty($search['release_date_under'])) {
            $where += array('release_date <=' => $search['release_date_under']);
        }
        return $where;
    }

    function create_like($search = '') {
        $like = array();
        if (!empty($search['song_title'])) {
            $like += array('song_title' => $search['song_title']);
        }
        if (!empty($search['lyricist'])) {
            $like += array('lyricist' => $search['lyricist']);
        }
        if (!empty($search['composer'])) {
            $like += array('composer' => $search['composer']);
        }
        if (!empty($search['singer'])) {
            $like += array('singer' => $search['singer']);
        }
        if (!empty($search['genre'])) {
            $like += array('genre' => $search['genre']);
        }
        return $like;
    }

    function insert_music($data) {
        $this->db->trans_start();
        $set_data = $this->create_set_data($data);
        $this->db->set($set_data);
        $this->db->insert('songs');
        $this->db->trans_complete();
    }

    function update_music($id, $data) {
        $this->db->trans_start();
        $set_data = $this->create_set_data($data);
        $this->db->set($set_data);
        $this->db->where('id', $id);
        $this->db->update('songs');
        $this->db->trans_complete();
    }

    private function create_set_data($data) {
        $set_data = array();
        $set_data = array(
            'song_title' => $data['song_title'],
            'lyricist' => $data['lyricist'],
            'composer' => $data['composer'],
            'singer' => $data['singer'],
            'genre' => $data['genre'],
            'release_date' => $data['release_date']
        );

        return $set_data;
    }

    function delete_music($id) {
        $this->db->trans_start();
        $this->db->where('id', $id);
        $this->db->delete('songs');
        $this->db->trans_complete();
    }

}
