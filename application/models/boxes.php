<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Boxes extends CI_Model {


    public function __construct() {
        parent::__construct();
        $this->load->database();
    }


     /**
      * 受け取ったIDの端末情報を返す
      */
    public function get_box($id) {
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->select('prefectures');
        $this->db->from('boxes');
        $this->db->where('id', $id);
        
        $query = $this->db->get();
        
        return $query->row();
    }

    

    /**
     * 端末一覧を返す
     */
    function get_list($search = '', $offset = ''){
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->select('prefectures');
        $this->db->from('boxes');
        $this->db->where($this->create_where($search));
        
        $query = $this->db->get();
        
        return $query->result();
    }


    /**
     * 受け取った配列からwhere句を作成し、返す
     */
    private function create_where($search) {
        $where = array();
        if (!empty($search['pref'])) {
            if ($search['pref'] != '指定しない') {
                $where += array('prefectures =' => $search['pref']);
            }
        }
        if (!empty($search['entry_date_over'])) {
            $where += array('entry_date >=' => $search['entry_date_over']);
        }
        if (!empty($search['entry_date_under'])) {
            $where += array('entry_date <=' => $search['entry_date_under']);
        }

        return $where;
    }


    /**
     *
     */
    public function add($newBox) {
        $sql = "insert into boxes (point, prefectures) ";
        $sql .= "values(geomFromText('point(". $newBox['x']. " ". $newBox['y']. ")'), ?)";
        $this->db->query($sql, array($newBox['prefectures']));
    }




}